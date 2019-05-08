.INTER_PANEL_D
0,10,"","RUN PC","","",10,4,15,1,pcexec fotorobotpc,0
1,10,"","STOP PC","","",10,4,15,2,stopcon = true,0
28,10,"","Start","setup","",10,4,15,3,pcexec 2: fotorobotsetup,0
37,3,"","","","",10,4,15,0,0,0,0,0
.END
.INTER_PANEL_TITLE
"Control",1
"Setup",0
"",0
"",0
"",0
"",0
"",0
"",0
.END
.INTER_PANEL_COLOR_D
182,3,224,244,28,159,252,255,251,255,0,31,2,241,52,219,
.END
.PROGRAM close_socket()  ;Closing communication 
    TCP_CLOSE ret,sock_id  ;Normal socket closure 
        IF ret < 0 THEN 
        PRINT "TCP_CLOSE error ERROE=(",ret,") ",$ERROR(ret) 
        TCP_CLOSE ret1,sock_id    ;Forced closure of socket (shutdown) 
        IF ret1 < 0 THEN 
            PRINT  "TCP_CLOSE error id=",sock_id 
        END 
    ELSE 
        PRINT  "TCP_CLOSE OK id=",sock_id 
    END 
    TCP_END_LISTEN ret,port 
        IF ret < 0 THEN 
        PRINT  "TCP_CLOSE error id=",sock_id 
    ELSE 
        PRINT  "TCP_CLOSE OK id=",sock_id 
    END 
.END 
.program fotorobot()
	CP OFF
	ACCURACY 0.1 ALWAYS
	$action = ""
	WHILE TRUE DO
		IF $action == "AIM" THEN
			$action = ""
			SPEED 30 ALWAYS
			JM #ap
		END
		IF $action == "HOME" THEN
			$action = ""
			SPEED 30 ALWAYS
			JM #hp
		END
		IF $action == "DRAW" THEN
			
			SPEED 30 ALWAYS
			
			drawed_points = 0
			WHILE TRUE DO
				IF drawed_points < points_length THEN
					IF $action != "STOPDRAW" THEN
						LMOVE TRANS(dpt[drawed_points,0],dpt[drawed_points,1],dpt[drawed_points,2])+startp
						drawed_points = drawed_points + 1
						$Percent = $ENCODE(/F6.2, drawed_points*100/points_length) + " %"
						; Returning to home point after finishing
					ELSE
						GOTO stop
					END
				ELSE	
					GOTO stop
				END
			END
stop:
				JMOVE #endp
				JMOVE #hp
				$State =  "STOP"

			$action = ""
					
		END
	END
.end
.PROGRAM fotorobotpc()
	port = 49152 
    max_length = 255 
    tout_open = 60 
    tout_rec = 5 
	text_id = 0 
    tout = 60 
    eret = 0 
	rret = 0
	stopCon = FALSE;
	;WHILE TRUE DO
con_begin:
	CALL open_socket     ;Connecting communication 
	IF sock_id < 0    THEN 
		GOTO  con_begin 
	END
	PRINT  "Connection established"
cyc_begin:
	IF stopCon == TRUE THEN
		GOTO exit
	END
	tout_rec = 5
	CALL recv        ;Receiving the result of processing 1 
	IF rret == -34024  THEN 
		PRINT  "Recieve timeout" 
		GOTO  cyc_begin 
	END
	IF rret == -34025  THEN 
		PRINT  "Connection error" 
		CALL close_socket
		GOTO  con_begin 
	END
	IF rret == 0 THEN
		
		PRINT $recv_buf[1]
		IF $recv_buf[1] == "AIM" THEN
			$action = "AIM"
		END
		IF $recv_buf[1] == "HOME" THEN
			$action = "HOME"
		END
		IF $recv_buf[1] == "DRAW" THEN
			tout_rec = 60 
			CALL recv
			points_length = VAL($recv_buf[1])
			PRINT  "Points len", points_length 
			
			eret = 0 
			$sdata[1] = "OK" 
			CALL  send(eret,$sdata[1])
			
			$fin = ""
			recieved_point = 0
			WHILE $fin == "" DO
				CALL recv
				IF $recv_buf[1] == "STOPDRAW" THEN
					$fin = "OK"
					$action = "STOPDRAW
					eret = 0 
					$sdata[1] = "OK" 
					CALL  send(eret,$sdata[1])
					GOTO recv_end
				END
				IF $recv_buf[1] == "ENDRECV" THEN
					$fin = "OK"
				ELSE
					FOR i = 0 TO 2 STEP 1
						$temp = $DECODE($recv_buf[1],":",0)
						dpt[recieved_point,i] = VAL($temp)
						PRINT  dpt[ recieved_point, i]
						$temp = $DECODE($recv_buf[1] ,":",1)
					END
					recieved_point = recieved_point + 1
					IF recieved_point*100/points_length > 5 THEN; recieved_point==50 THEN
						$action = "DRAW"
					END
					
					$Uploaded = $ENCODE(/F6.2, recieved_point*100/points_length) + " %"
				END
recv_end:
				eret = 0 
				$sdata[1] = "OK" 
				CALL  send(eret,$sdata[1])
				
			END
			
		END
	
	END
	eret = 0 
    $sdata[1] = "Resp" 
    CALL  send(eret,$sdata[1])
	
	
	GOTO cyc_begin
    ;END    
exit: 
    CALL    close_socket   ;Closing communication 
exit_end:
.END
.PROGRAM main()  ;Communication main program 
    port = 49152 
    max_length = 255 
    tout_open = 60 
    tout_rec = 60 
    CALL open_socket     ;Connecting communication 
IF sock_id < 0    THEN 
        GOTO  exit_end 
    END 
    text_id = 0 
    tout = 60 
    eret = 0 
rret = 0 
    $sdata[1] = "001" 
    CALL  send(eret,$sdata[1])  ;Instructing processing 1 
    IF eret < 0  THEN 
        PRINT  "CODE 001 ERROR END code=",eret 
        GOTO  exit 
    END 
    CALL recv        ;Receiving the result of processing 1 
    IF rret < 0  THEN 
        PRINT  "CODE 001 RECV ERROR END code=",rret 
        GOTO  exit 
    END 
    eret = 0 
    $sdata[1] = "002" 
    CALL  send(eret,$sdata[1])  ;Instructing processing 2 
    IF eret < 0  THEN 
        PRINT  "CODE 002 ERROR END code=",eret 
        GOTO  exit 
    END 
    CALL recv        ;Receiving the result of processing 2 
    IF rret < 0  THEN 
        PRINT  "CODE 002 RECV ERROR END code=",rret 
        GOTO  exit 
    END 
exit: 
    CALL    close_socket   ;Closing communication 
exit_end: 
.END 
.PROGRAM open_socket() ;Starting c ommunication 
    er_count =0 
listen: 
    TCP_LISTEN retl,port 
    IF retl<0  THEN 
        IF er_count >= 5  THEN 
            PRINT  "Connection with PC is failed (LISTEN). Program is stopped." 
            sock_id = -1 
            goto  exit 
        ELSE 
            er_count = er_count+1 
            PRINT  "TCP_LISTEN error=",retl,"    error count=",er_count 
            GOTO  listen 
        END 
    ELSE 
        PRINT  "TCP_LISTEN OK ",retl 
    END 
    er_count =0 
accept: 
    TCP_ACCEPT sock_id,port,tout_open,ip[1] 
    IF sock_id<0 THEN 
        IF er_count >= 5  THEN 
            PRINT  "Connection with PC is failed (ACCEPT). Program is stopped." 
            TCP_END_LISTEN  ret,port 
            sock_id = -1 
        ELSE 
            er_count = er_count+1 
            PRINT  "TCP_ACCEPT error id=",sock_id,"  error count=",er_count 
            GOTO  accept 
        END 
    ELSE 
        PRINT  "TCP_ACCEPT OK id=",sock_id 
    END 
exit: 
.END 
.PROGRAM recv() ;Communication  Receiving data 
    .num=0 
    TCP_RECV rret,sock_id,$recv_buf[1],.num,tout_rec,max_length 
    IF rret < 0  THEN 
        PRINT  "TCP_RECV error in RECV",rret 
        $recv_buf[1]="000" 
    ELSE 
        IF .num > 0  THEN 
            PRINT  "TCP_RECV OK  in RECV",rret 
        ELSE 
            $recv_buf[1]="000" 
        END 
    END 
.END 
.PROGRAM send(.ret,.$data)    ;Communication  Sending data 
    $send_buf[1] = .$data 
    buf_n = 1 
  .ret = 1 
    TCP_SEND .ret,sock_id,$send_buf[1],buf_n,tout 
    IF .ret < 0 THEN 
        .ret = -1 
        PRINT  "TCP_SEND error in SEND",.ret 
    ELSE 
        PRINT  "TCP_SEND OK  in SEND",.ret 
    END 
.END 
.PROGRAM fotorobotsetup()
  
  
  
.END
.PROGRAM Comment___ () ; Comments for IDE. Do not use.
	; @@@ PROJECT @@@
	; @@@ HISTORY @@@
	; 08.05.2019 01:09:48
	; 
	; @@@ INSPECTION @@@
	; @@@ CONNECTION @@@
	; 
	; 
	; 
	; @@@ PROGRAM @@@
	; 0:close_socket
	; 0:fotorobot
	; 0:fotorobotpc
	; 0:main
	; 0:open_socket
	; 0:recv
	; 0:send
	; 0:fotorobotsetup
	; @@@ TRANS @@@
	; @@@ JOINTS @@@
	; @@@ REALS @@@
	; @@@ STRINGS @@@
	; @@@ INTEGER @@@
	; @@@ SIGNALS @@@
	; @@@ TOOLS @@@
	; @@@ BASE @@@
	; @@@ FRAME @@@
	; @@@ BOOL @@@
.END