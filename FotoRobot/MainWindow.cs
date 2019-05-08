using Emgu.CV;
using Emgu.CV.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FotoRobot
{
    public partial class MainWindow : Form
    {
        VideoCapture Camera;
        Mat ROIFrame;
        Size FrameSize;
        Rectangle ROI;
        List<List<PointF>> Contours;

        Parameters parameters;

        bool isCaptureRequired;
        bool isFrameRequired;
        bool isProcessingRequired;
        bool isPointsSendingRequired;

        bool killThreads = false;

        TcpClient TCP;
        NetworkStream networkStream;// = TCP.GetStream();

        #region THREADS
        Thread tcpCheck;
        Thread pointsSender;
        Thread cameraThread;
        Thread processingThread;
        Thread receiveThread;

        bool isSendSuccessful;
        private void receiveThreadFunction()
        {
            while(!killThreads)
            {
                if (TCP != null)
                {
                    try
                    {
                        byte[] buffer = new byte[1024];
                        networkStream.ReadTimeout = 10000;
                        networkStream.Read(buffer, 0, 1024);
                        String rec = buffer.ToString();
                        rec = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                        if (rec.Contains("DRAWOK"))
                        {
                            if (InvokeRequired) { Invoke(new MethodInvoker(() => {
                                buttonDraw.Image = Properties.Resources.paint;
                                buttonCapture.Enabled = true;
                            })); }

                        }
                        if (rec.Contains("OK"))
                        {
                            isSendSuccessful = true;
                        }
                        int a = 1;
                    } catch (Exception exp) {

                        isSendSuccessful = true;
                    }
                }
            }
        }

        private void pointsSenderFunction()
        {
            while (!killThreads)
            {
                if (isPointsSendingRequired)
                {
                    sendingRequiredFunction();
                    isPointsSendingRequired = false;
                }
            }
        }

        private void cameraThreadFunction()
        {
            while (!killThreads)
            {

                if (isCaptureRequired)
                {
                    updateParameters();
                    captureRequiredFunction();
                }
            }
        }

        private void processingThreadFunction()
        {
            while (!killThreads)
            {
                if (isProcessingRequired)
                {
                    updateParameters();
                    processingRequiredFunction();
                }
            }
        }

        private void TCPCheckFunction()
        {
            while (!killThreads)
            {
                if (TCP != null)
                {
                    if (conStatus.InvokeRequired) { conStatus.Invoke(new MethodInvoker(() => { conStatus.Text = "CONNECTED"; })); }

                    if (TCP != null && !isClientConnected())
                    {
                        if (conStatus.InvokeRequired) { conStatus.Invoke(new MethodInvoker(() => { conStatus.Text = "DISCONNECTED"; })); }
                        TCP = null;
                    }
                }
                else
                {
                    setupTCP();
                }
            }
        }

        #endregion

        #region THREADFUNCTIONS
        private void sendingRequiredFunction()
        {
            List<Point3F> points = getSendingVector();

            TCPSend("DRAW");
            waitResponse();
            TCPSend((points.Count).ToString());
            waitResponse();

            for (int i = 0; i < points.Count; i++)
            {
                if (isPointsSendingRequired)
                {
                    String msg = points[i].X.ToString() + ":" + points[i].Y.ToString() + ":" + (points[i].Z).ToString() + ":";
                    TCPSend(msg);
                    waitResponse();
                }
                else
                {
                    TCPSend("STOPDRAW");
                    waitResponse();
                    return;
                }
            }
            TCPSend("ENDRECV");
        }

        private List<Point3F> getSendingVector()
        {
            List<Point3F> points = new List<Point3F>();
            Point3F temp;
            for (int i = 0; i < Contours.Count; i++)
            {
                temp.X = Contours[i][0].X;
                temp.Y = Contours[i][0].Y;
                temp.Z = 15;
                points.Add(temp);

                for (int j = 0; j < Contours[i].Count; j++)
                {

                    temp.X = Contours[i][j].X;
                    temp.Y = Contours[i][j].Y;
                    temp.Z = 0;
                    points.Add(temp);
                }
                temp.X = Contours[i][Contours[i].Count - 1].X;
                temp.Y = Contours[i][Contours[i].Count - 1].Y;
                temp.Z = 15;
                points.Add(temp);
            }
            return points;
        }

        private void captureRequiredFunction()
        {
            isProcessingRequired = false;
            Mat temp = new Mat();// = Camera.QueryFrame();
            Camera.Read(temp);
            ROI.Size = parameters.ROISize;//new Size(ROIWidth.Value, ROIHeight.Value);
            cutROI();
            if (isFrameRequired)
            {
                ROIFrame = new Mat(temp, ROI);
                preprocessROIFrame();

                isCaptureRequired = false;
                isFrameRequired = false;
                isProcessingRequired = true;
            }
            else
            {
                CvInvoke.Rectangle(temp, ROI, new Emgu.CV.Structure.MCvScalar(0, 255, 0), 2);
                if (imageView.InvokeRequired) { imageView.Invoke(new MethodInvoker(() => { imageView.Image = temp; })); }
                //imageView.Image = temp;
            }
        }

        private void processingRequiredFunction()
        {
            Mat CannyFrame = getCannyImage();
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            VectorOfVectorOfPoint smallContours = new VectorOfVectorOfPoint();
            List<List<PointF>> scaledContours = new List<List<PointF>>();

            contours = getContours(CannyFrame);
            smallContours = getSmallContours(contours);
            drawContours(smallContours);

            double scale = getImageScale();
            //scaledContours = smallContours;
            for (int i = 0; i < smallContours.Size; i++)
            {
                List<PointF> temp = new List<PointF>();
                for (int j = 0; j < smallContours[i].Size; j++)
                {
                    temp.Add(new PointF((float)((double)smallContours[i][j].X * scale),
                        (float)((double)smallContours[i][j].Y * scale)));
                    //scaledContours[i][j] = new Point(smallContours[i][j].X * scale, smallContours[i][j].Y*scale);
                }
                scaledContours.Add(temp);
            }
            Contours = scaledContours;

        }
        #endregion

        #region UI
        private void buttonCamera_Click(object sender, EventArgs e)
        {
            if (isCaptureRequired)
            {
                isCaptureRequired = false;
                buttonCamera.Image = Properties.Resources.play;
                buttonCapture.Enabled = false;

            }
            else
            {
                isCaptureRequired = true;
                buttonCamera.Image = Properties.Resources.stop;
                buttonCapture.Enabled = true;

                TCPSend("AIM");

                //networkStream.read

            }
            buttonDraw.Enabled = false;
        }

        private void buttonCapture_Click(object sender, EventArgs e)
        {
            isFrameRequired = true;
            buttonDraw.Enabled = true;
            buttonCapture.Enabled = false;
            buttonCamera.Image = Properties.Resources.play;
            TCPSend("HOME");
        }

        private void buttonSliders_Click(object sender, EventArgs e)
        {
            if (mainLayoutPanel.ColumnStyles[2].Width == 0)
            {
                mainLayoutPanel.ColumnStyles[2].Width = 150;
            }
            else
            {
                mainLayoutPanel.ColumnStyles[2].Width = 0;
            }

        }

        Point prev_point = new Point(0, 0);
        private void imageView_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ROI.Location = Point.Add(Point.Subtract(e.Location, (Size)prev_point), (Size)ROI.Location);
            }
            prev_point = e.Location;
        }

        private void imageView_MouseDown(object sender, MouseEventArgs e)
        {
            prev_point = e.Location;
        }

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {

            killThreads = true;
            cameraThread.Abort();
            processingThread.Abort();
            pointsSender.Abort();
            tcpCheck.Abort();
            receiveThread.Abort();
            Thread.Sleep(1000);

            SaveSettings();
        }

        private void buttonDraw_Click(object sender, EventArgs e)
        {
            if (!isPointsSendingRequired)
            {
                isPointsSendingRequired = true;
                buttonCapture.Enabled = false;
                buttonDraw.Image = Properties.Resources.stop;
                saveImage();
            }
            else
            {
                isPointsSendingRequired = false;
                buttonCapture.Enabled = true;
                buttonDraw.Image = Properties.Resources.paint;
            }
        }

        private void buttonSettings_Click(object sender, EventArgs e)
        {
            SettingsTransfer.paperSize = parameters.paperSize;
            SettingsTransfer.cameraResolution = parameters.cameraResolution;
            if (new SettingsWindow().ShowDialog() == DialogResult.OK)
            {
                parameters.cameraResolution = SettingsTransfer.cameraResolution;
                parameters.paperSize = SettingsTransfer.paperSize;
            }
        }

        private void buttonLoad_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                Mat temp = new Mat();
                ROIFrame = CvInvoke.Imread(fileDialog.FileName);
                //CvInvoke.Flip(temp, ROIFrame, Emgu.CV.CvEnum.FlipType.None);
                preprocessROIFrame();
                isProcessingRequired = true;
                buttonDraw.Enabled = true;
            }
        }

        #endregion

        public MainWindow()
        {
            InitializeComponent();
            Initialize();
            HelpButton = true;
            TCP = null;
            setupTCP();
            tcpCheck = new Thread(TCPCheckFunction);
            tcpCheck.Start();
            pointsSender = new Thread(pointsSenderFunction);
            pointsSender.Start();
            cameraThread = new Thread(cameraThreadFunction);
            cameraThread.Start();
            processingThread = new Thread(processingThreadFunction);
            processingThread.Start();
            receiveThread = new Thread(receiveThreadFunction);
            receiveThread.Start();
            //cameraThread = new Thread(cameraThreadFunction);
            //cameraThread.Start();
            
            
            conStatus.Text = "DISCONNECTED";

        }

        private void Initialize()
        {
            isCaptureRequired = false;
            isFrameRequired = false;
            isProcessingRequired = false;
            isPointsSendingRequired = false;

            Contours = new List<List<PointF>>();
            parameters.cameraResolution = new Size(800, 600);
            parameters.paperSize = new Size(210, 297);
            LoadSettings();
            setupCamera();
            ROIFrame = new Mat();
        }

        public void updateParameters()
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() =>
                {
                    parameters.cannyThreshold1 = cannyThreshold1.Value;
                    parameters.cannyThreshold2 = cannyThreshold2.Value;
                    parameters.blurValue = Convert.ToInt32(BlurValue.SelectedItem);
                    parameters.approximationEpsilon = ApproximationEpsilon.Value;
                    parameters.minContLength = MinContLength.Value;
                    parameters.isAdaptive = isAdaptiveCanny.Checked;
                    parameters.ROISize.Width = ROIWidth.Value;
                    parameters.ROISize.Height = ROIHeight.Value;
                }));

            }
        }

        protected override bool ProcessCmdKey(ref Message message, Keys keys)
        {
            switch (keys)
            {
                case Keys.Space:
                    {
                        buttonCamera_Click(new object(), new EventArgs());
                        return true;
                    }

                case Keys.Enter:
                    {
                        if (isProcessingRequired)
                        {
                            buttonDraw_Click(new object(), new EventArgs());
                        } else
                        {
                            buttonCapture_Click(new object(), new EventArgs());
                        }
                        return true;
                    }
                    // signal that we've processed this key
            }

            // run base implementation
            return base.ProcessCmdKey(ref message, keys);
        }

        private void setupTCP()
        {
            try
            {
                TCP = new TcpClient("192.168.0.2", 49152);
                networkStream = TCP.GetStream();
                //clientStreamWriter = new StreamWriter(networkStream);
            }
            catch (Exception exp)
            {
                TCP = null;
            }
        }

        private void waitResponse()
        {
            isSendSuccessful = false;
            while (!isSendSuccessful) ;
        }

        private void preprocessROIFrame()
        {
            if (ROIFrame.Width > ROIFrame.Height)
            {
                CvInvoke.Rotate(ROIFrame, ROIFrame, Emgu.CV.CvEnum.RotateFlags.Rotate90Clockwise);
            }
            CvInvoke.Flip(ROIFrame, ROIFrame, Emgu.CV.CvEnum.FlipType.None);
        }

        private VectorOfVectorOfPoint getSmallContours(VectorOfVectorOfPoint contours)
        {
            VectorOfVectorOfPoint smallContours = new VectorOfVectorOfPoint();

            for (int i = 0; i < contours.Size; i++)
            {
                if (getContourLength(contours[i]) >= MinContLength.Value)
                {
                    smallContours.Push(contours[i]);
                }
            }
            for (int i = 0; i < smallContours.Size; i++)
            {

                double eps = (double)parameters.approximationEpsilon/100.0;// / 100.0;
                if (smallContours[i].Size != 0)
                {
                    CvInvoke.ApproxPolyDP(smallContours[i], smallContours[i], eps, false);
                }

            }
            return smallContours;
        }
      
        private void drawContours(VectorOfVectorOfPoint contours)
        {
            Mat Canvas = Mat.Zeros(ROIFrame.Rows, ROIFrame.Cols, Emgu.CV.CvEnum.DepthType.Cv8U, 1);
            CvInvoke.BitwiseNot(Canvas, Canvas);
            CvInvoke.DrawContours(Canvas, contours, -1, new Emgu.CV.Structure.MCvScalar(0, 0, 0), 1);
            CvInvoke.Flip(Canvas, Canvas, Emgu.CV.CvEnum.FlipType.None);
            imageView.Image = Canvas;

        }

        private Mat getCannyImage()
        {
            Mat GrayFrame = new Mat();
            Mat BlurFrame = new Mat();
            Mat CannyFrame = new Mat();

            CvInvoke.CvtColor(ROIFrame, GrayFrame, Emgu.CV.CvEnum.ColorConversion.Bgr2Gray);
            CvInvoke.MedianBlur(GrayFrame, BlurFrame, parameters.blurValue);

            double CannyThresh2 = parameters.cannyThreshold1;
            double CannyThresh1 = parameters.cannyThreshold2;
            if (parameters.isAdaptive)
            {
                CannyThresh2 = CvInvoke.Threshold(BlurFrame, CannyFrame, 0, 255,
                    Emgu.CV.CvEnum.ThresholdType.Binary | Emgu.CV.CvEnum.ThresholdType.Otsu);
                CannyThresh1 = 0.1 * CannyThresh2;
                if (InvokeRequired)
                {
                    Invoke(new MethodInvoker(() =>
                    {
                        cannyThreshold1.Value = (int)CannyThresh1;
                        cannyThreshold2.Value = (int)CannyThresh2;
                    }));
                }
            }
            

            CvInvoke.Canny(BlurFrame, CannyFrame, CannyThresh1, CannyThresh2);
            return CannyFrame;
        }

        private VectorOfVectorOfPoint getContours(Mat img)
        {
            VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
            IOutputArray hierarchy = null;
            CvInvoke.FindContours(img, contours, hierarchy, 
                Emgu.CV.CvEnum.RetrType.Tree, 
                Emgu.CV.CvEnum.ChainApproxMethod.ChainApproxTc89Kcos);
            return contours;
        }

        private double getContourLength(VectorOfPoint contour)
        {
            double length = 0;
            for (int i = 1; i < contour.Size; i++)
            {
                length += Math.Sqrt(Math.Pow(contour[i].X - contour[i - 1].X, 2) +
                    Math.Pow(contour[i].Y - contour[i - 1].Y, 2));
            }
            return length;// / contour.Size;
        }

        private double getImageScale()
        {
            double scaleX = (double)(parameters.paperSize.Width) / (double)ROIFrame.Cols;
            double scaleY = (double)(parameters.paperSize.Height) / (double)ROIFrame.Rows;
            return (scaleX < scaleY) ? scaleX : scaleY;
        }

        private void cutROI()
        {
            ROI.X = (ROI.X < 0) ? 0 : ROI.X;
            ROI.Y = (ROI.Y < 0) ? 0 : ROI.Y;
            ROI.Width = (ROI.X + ROI.Width >= FrameSize.Width) ? ROI.Width = FrameSize.Width - ROI.X - 2 : ROI.Width;
            ROI.Height = (ROI.Y + ROI.Height >= FrameSize.Height) ? ROI.Height = FrameSize.Height - ROI.Y - 2 : ROI.Height;

        }

        private void LoadSettings()
        {
            ROI.Location = Properties.Settings.Default.ROIPosition;
            ROI.Size = Properties.Settings.Default.ROISize;
            ROIWidth.Value = Properties.Settings.Default.ROISize.Width;
            ROIHeight.Value = Properties.Settings.Default.ROISize.Height;
            cannyThreshold1.Value = Properties.Settings.Default.cannyThreshold1;
            cannyThreshold2.Value = Properties.Settings.Default.cannyThreshold2;
            BlurValue.SelectedIndex = Properties.Settings.Default.blurValue;
            isAdaptiveCanny.Checked = Properties.Settings.Default.isAdaptive;
            ApproximationEpsilon.Value = Properties.Settings.Default.approximationEpsilon;
            MinContLength.Value = Properties.Settings.Default.minContLen;
            parameters.paperSize = Properties.Settings.Default.paperSize;
            parameters.cameraResolution = Properties.Settings.Default.cameraResolution;
            

        }

        private void SaveSettings()
        {
            Properties.Settings.Default.ROIPosition = ROI.Location;
            Properties.Settings.Default.ROISize = ROI.Size;
            Properties.Settings.Default.cannyThreshold1 = cannyThreshold1.Value;
            Properties.Settings.Default.cannyThreshold2 = cannyThreshold2.Value;
            Properties.Settings.Default.blurValue = BlurValue.SelectedIndex;
            Properties.Settings.Default.isAdaptive = isAdaptiveCanny.Checked;
            Properties.Settings.Default.approximationEpsilon = ApproximationEpsilon.Value;
            Properties.Settings.Default.minContLen = MinContLength.Value;
            Properties.Settings.Default.cameraResolution = parameters.cameraResolution;
            Properties.Settings.Default.paperSize = parameters.paperSize;
            Properties.Settings.Default.Save();
        }

        private bool setupCamera()
        {
            Camera = new VideoCapture();
            Camera.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth,
                parameters.cameraResolution.Width);
            Camera.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameHeight,
                parameters.cameraResolution.Height);

            Mat temp = new Mat();
            Camera.Read(temp);
            if (!Camera.Grab())
            {
                MessageBox.Show("Unable to open camera!", "Error", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (temp.Size != parameters.cameraResolution)
            {
                MessageBox.Show("Camera resolution " + parameters.cameraResolution + 
                    " doesn't fit given resolution: " + temp.Size, "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            FrameSize = temp.Size;
            return true;
        }

        private void TCPSend(String msg)
        {
            try
            {
                byte[] responseByte = ASCIIEncoding.ASCII.GetBytes(msg);
                networkStream.Write(responseByte, 0, responseByte.Length);
            } catch (Exception exp) { }
        }

        public bool isClientConnected()
        {
            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();

            TcpConnectionInformation[] tcpConnections = ipProperties.GetActiveTcpConnections();

            foreach (TcpConnectionInformation c in tcpConnections)
            {
                TcpState stateOfConnection = c.State;
                
                if (c.LocalEndPoint.Equals(TCP.Client.LocalEndPoint) && c.RemoteEndPoint.Equals(TCP.Client.RemoteEndPoint))
                {
                    if (stateOfConnection == TcpState.Established)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }

                }

            }

            return false;


        }
       
        private void checkSaveFolderExist(String path)
        {
            //String path = "pics";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private void saveImage()
        {
            String path = "pics";
            checkSaveFolderExist(path);
            String date = DateTime.Now.ToShortDateString() + "-"
                + DateTime.Now.ToLongTimeString().Replace(':', '.');
            path = path + "/" + date + ".jpg";
            Mat temp = new Mat();
            
            CvInvoke.Flip(ROIFrame, temp, Emgu.CV.CvEnum.FlipType.None);
            CvInvoke.Imwrite(path, temp);

        }

        private void isAdaptiveCanny_CheckedChanged(object sender, EventArgs e)
        {
            if (isAdaptiveCanny.Checked)
            {
                cannyThreshold1.Enabled = false;
                cannyThreshold2.Enabled = false;
            }
            else
            {
                cannyThreshold1.Enabled = true;
                cannyThreshold2.Enabled = true;
            }
        }

    }







}
