#include "sys.h"
#include "delay.h"
#include "usart.h" 
#include "led.h" 		 	 
#include "lcd.h"  
#include "key.h"     
#include "usmart.h" 
#include "malloc.h"
#include "sdio_sdcard.h"  
#include "w25qxx.h"    
#include "ff.h"  
#include "exfuns.h"   
#include "text.h"
#include "piclib.h"	
#include "string.h"		
#include "math.h"	 
#include "ov7670.h" 
#include "beep.h" 
#include "timer.h" 
#include "exti.h"
 
 
/************************************************
 ALIENTEK��ӢSTM32������ʵ��39
 ����� ʵ�� 
 ����֧�֣�www.openedv.com
 �Ա����̣�http://eboard.taobao.com 
 ��ע΢�Ź���ƽ̨΢�źţ�"����ԭ��"����ѻ�ȡSTM32���ϡ�
 ������������ӿƼ����޹�˾  
 ���ߣ�����ԭ�� @ALIENTEK
************************************************/


extern u8 ov_sta;	//��exit.c���涨��
extern u8 ov_frame;	//��timer.c���涨��	   
//����LCD��ʾ
void camera_refresh(void)
{
	u32 j;
 	u16 color;	 
	if(ov_sta)//��֡�жϸ��£�
	{
		LCD_Scan_Dir(U2D_L2R);		//���ϵ���,������  
		if(lcddev.id==0X1963)LCD_Set_Window((lcddev.width-240)/2,(lcddev.height-320)/2,240,320);//����ʾ�������õ���Ļ����
		else if(lcddev.id==0X5510||lcddev.id==0X5310)LCD_Set_Window((lcddev.width-320)/2,(lcddev.height-240)/2,320,240);//����ʾ�������õ���Ļ����
		LCD_WriteRAM_Prepare();     //��ʼд��GRAM	
		OV7670_RRST=0;				//��ʼ��λ��ָ�� 
		OV7670_RCK_L;
		OV7670_RCK_H;
		OV7670_RCK_L;
		OV7670_RRST=1;				//��λ��ָ����� 
		OV7670_RCK_H;
		for(j=0;j<76800;j++)
		{
			OV7670_RCK_L;
			color=GPIOC->IDR&0XFF;	//������
			OV7670_RCK_H; 
			color<<=8;  
			OV7670_RCK_L;
			color|=GPIOC->IDR&0XFF;	//������
			OV7670_RCK_H; 
			LCD->LCD_RAM=color;    
		}   							  
 		ov_sta=0;					//����֡�жϱ��
		ov_frame++; 
		LCD_Scan_Dir(DFT_SCAN_DIR);	//�ָ�Ĭ��ɨ�跽�� 
	} 
}	   
//�ļ������������⸲�ǣ�
//��ϳ�:����"0:PHOTO/PIC13141.bmp"���ļ���
void camera_new_pathname(u8 *pname)
{	 
	u8 res;					 
	u16 index=0;
	while(index<0XFFFF)
	{
		sprintf((char*)pname,"0:PHOTO/PIC%05d.bmp",index);
		res=f_open(ftemp,(const TCHAR*)pname,FA_READ);//���Դ�����ļ�
		if(res==FR_NO_FILE)break;		//���ļ���������=����������Ҫ��.
		index++;
	}
}
//�ļ�����-������ɫ-lcd��ʾ
u8 stdbmp_send(const u8 *filename) 
{
	FIL* f_bmp;  //���ʣ��ļ���ַ  ָ��һ���ļ��ı���
    u16 br;		 //���ƣ��Ѿ��������ֽ���

    u16 count;					//���ƣ���������    	   
	u8  rgb ,color_byte; 	//���ƣ���ɫ����ɫ��������ɫ��
	u16 x ,y,color;	  		//���ƣ�x��y�����꣬��ɫ
	u16 countpix=0;				//���ƣ���¼���� 	 

	//���ƣ�x,y��ʵ������	
	u16  realx=0;
	u16 realy=0;
	u8  yok=1;  				   
	u8 res;
	u16 r;  //ÿһ�����ص�red����
	u16 g;		//green����
	u16 b;		//blue����
	u8 gray;

	u8 *databuf;    					//���ݣ����ݶ�ȡ��ŵ�ַ
 	u16 readlen=BMP_DBUF_SIZE;//���ƣ�һ�δ�SD����ȡ���ֽ�������

	u8 *bmpbuf;			  	//���ʣ����ݽ����ַ
	u8 biCompression=0;	//���ʣ���¼ѹ����ʽ
	
	u16 rowlen;	  		 	//���ʣ�ˮƽ�����ֽ���  
	BITMAPINFO *pbmp;		//��ʱָ�� ָʾbitmapinfo ͷ����Ϣ
	
#if BMP_USE_MALLOC == 1	//ʹ��malloc	
	databuf=(u8*)pic_memalloc(readlen);		//����readlen�ֽڵ��ڴ�����
	if(databuf==NULL)return PIC_MEM_ERR;	//�ڴ�����ʧ��.
	f_bmp=(FIL *)pic_memalloc(sizeof(FIL));	//����FIL�ֽڵ��ڴ����� 
	if(f_bmp==NULL)							//�ڴ�����ʧ��.
	{		 
		pic_memfree(databuf);
		return PIC_MEM_ERR;				
	} 	 
#else				 	//��ʹ��malloc
	databuf=bmpreadbuf;
	f_bmp=&f_bfile;
#endif
	res=f_open(f_bmp,(const TCHAR*)filename,FA_READ);//���ļ�	 						  
	if(res==0)//�򿪳ɹ�.
	{ 
		f_read(f_bmp,databuf,readlen,(UINT*)&br);	//����readlen���ֽ�  
		pbmp=(BITMAPINFO*)databuf;					//�õ�BMP��ͷ����Ϣ   
		count=pbmp->bmfHeader.bfOffBits;        	//����ƫ��,�õ����ݶεĿ�ʼ��ַ
		color_byte=pbmp->bmiHeader.biBitCount/8;	//��ɫλ 16/24/32  
		biCompression=pbmp->bmiHeader.biCompression;//ѹ����ʽ
		picinfo.ImgHeight=pbmp->bmiHeader.biHeight;	//�õ�ͼƬ�߶�
		picinfo.ImgWidth=pbmp->bmiHeader.biWidth;  	//�õ�ͼƬ��� 
		ai_draw_init();//��ʼ�����ܻ�ͼ			
		//ˮƽ���ر�����4�ı���!!
		if((picinfo.ImgWidth*color_byte)%4)rowlen=((picinfo.ImgWidth*color_byte)/4+1)*4;
		else rowlen=picinfo.ImgWidth*color_byte;
		//��ʼ����BMP   
		color=0;//��ɫ���	 													 
		x=0 ;
		y=picinfo.ImgHeight;
		rgb=0;      
		//���ڳߴ�С�ڵ����趨�ߴ��ͼƬ,���п��ٽ���
		realy=(y*picinfo.Div_Fac)>>13;
		bmpbuf=databuf;
		while(1)
		{				 
			while(count<readlen)  //��ȡһ��1024���� (SectorsPerClust ÿ��������)
		    {
					if(color_byte==3)   //24λ��ɫͼ
				{
					switch (rgb) 
					{
						case 0:				  
							color=bmpbuf[count]>>3; //B
							break ;	   
						case 1: 	 
							color+=((u16)bmpbuf[count]<<3)&0X07E0;//G
							break;	  
						case 2 : 
							color+=((u16)bmpbuf[count]<<8)&0XF800;//R	  
							break ;			
					}   
				}else if(color_byte==2)  //16λ�� ɫͼ
				{
					switch(rgb)
					{
						case 0 : 
							if(biCompression==BI_RGB)//RGB:5,5,5
							{
								color=((u16)bmpbuf[count]&0X1F);	 	//R
								color+=(((u16)bmpbuf[count])&0XE0)<<1; //G
							}else		//RGB:5,6,5
							{
								color=bmpbuf[count];  			//G,B
							}  
							break ;   
						case 1 : 			  			 
							if(biCompression==BI_RGB)//RGB:5,5,5
							{
								color+=(u16)bmpbuf[count]<<9;  //R,G
							}else  		//RGB:5,6,5
							{
								color+=(u16)bmpbuf[count]<<8;	//R,G
							}  									 
							break ;	 
					}		     
				}else if(color_byte==4)//32λ��ɫͼ
				{
					switch (rgb)
					{
						case 0:				  
							color=bmpbuf[count]>>3; //B
							break ;	   
						case 1: 	 
							color+=((u16)bmpbuf[count]<<3)&0X07E0;//G
							break;	  
						case 2 : 
							color+=((u16)bmpbuf[count]<<8)&0XF800;//R	  
							break ;			
						case 3 :
							//alphabend=bmpbuf[count];//����ȡ  ALPHAͨ��
							break ;  		  	 
					}	
				}else if(color_byte==1)//8λɫ,��ʱ��֧��,��Ҫ�õ���ɫ��.
				{
				} 
				rgb++;	  
				count++ ;		  
				if(rgb==color_byte) //ˮƽ�����ȡ��1���������ݺ���ʾ  Ҫ�����е���ɫ�����������Ž�����ʾ
				{	
					if(x<picinfo.ImgWidth)	 					 			   
					{	
						realx=(x*picinfo.Div_Fac)>>13;//x��ʵ��ֵ
						if(is_element_ok(realx,realy,1)&&yok)//��������
						{						 				 	  	       
							//pic_phy.draw_point(realx+picinfo.S_XOFF,realy+picinfo.S_YOFF-1,color);//��ʾͼƬ	 
							r = color&0xf800;
							r = color>>11;
							g =color&0x07e0;
							g>>=5;				
							b = color&0x001f;
							//gray=r/3+g/3+b/3;
							gray=(r*30+g*59+b*11)*0.01f;
							USART_SendData(USART1, gray);//�򴮿�1��������
							while(USART_GetFlagStatus(USART1,USART_FLAG_TC)!=SET);//�ȴ����ͽ���
							delay_us(10);
						}   									    
					}
					x++;//x������һ������ 
					color=0x0000; 
					rgb=0;  		  
				}
				countpix++;//�����ۼ�
				
				if(countpix>=rowlen)//ˮƽ��������ֵ����.����
				{		 
					y--; 
					if(y==0)break;			 
					realy=(y*picinfo.Div_Fac)>>13;//ʵ��yֵ�ı�	 
					if(is_element_ok(realx,realy,0))yok=1;//�˴����ı�picinfo.staticx,y��ֵ	 
					else yok=0; 
					x=0; 
					countpix=0;
					color=0x0000;
					rgb=0;
				}	 
			} 		
			res=f_read(f_bmp,databuf,readlen,(UINT *)&br);//����readlen���ֽ�
			if(br!=readlen)readlen=br;	//���һ������		  
			if(res||br==0)break;		//��ȡ����
			bmpbuf=databuf;
	 	 	count=0;
		}  
		f_close(f_bmp);//�ر��ļ�
	}  	
#if BMP_USE_MALLOC == 1	//ʹ��malloc	
	pic_memfree(databuf);	 
	pic_memfree(f_bmp);		 
#endif	
	return res;		//BMP��ʾ����.    					   
}	



 int main(void)
 {	 
	u8 res;							 
	u8 *pname;				//��·�����ļ��� 
	u8 key;					//��ֵ		   
	u8 i;						 
	u8 sd_ok=1;				//0,sd��������;1,SD������.
	 
	delay_init();	    	 //��ʱ������ʼ��	  
  NVIC_PriorityGroupConfig(NVIC_PriorityGroup_2);//�����ж����ȼ�����Ϊ��2��2λ��ռ���ȼ���2λ��Ӧ���ȼ�
	uart_init(115200);	 	//���ڳ�ʼ��Ϊ115200
 	usmart_dev.init(72);		//��ʼ��USMART		
 	LED_Init();		  			//��ʼ����LED���ӵ�Ӳ���ӿ�
	KEY_Init();					//��ʼ������
	LCD_Init();			   		//��ʼ��LCD    
	BEEP_Init();        		//��������ʼ��	 
	W25QXX_Init();				//��ʼ��W25Q128
 	my_mem_init(SRAMIN);		//��ʼ���ڲ��ڴ��
	exfuns_init();				//Ϊfatfs��ر��������ڴ�  
 	f_mount(fs[0],"0:",1); 		//����SD�� 
 	f_mount(fs[1],"1:",1); 		//����FLASH. 
	POINT_COLOR=RED;      
 	while(font_init()) 				//����ֿ�
	{	    
		LCD_ShowString(30,50,200,16,16,"Font Error!");
		delay_ms(200);				  
		LCD_Fill(30,50,240,66,WHITE);//�����ʾ	     
	}  	 
 	Show_Str(30,50,200,16,"��ӢSTM32F1������",16,0);				    	 
	Show_Str(30,70,200,16,"�����ʵ��",16,0);				    	 
	Show_Str(30,90,200,16,"KEY0:����",16,0);				    	 
	res=f_mkdir("0:/PHOTO");		//����PHOTO�ļ���
	if(res!=FR_EXIST&&res!=FR_OK) 	//�����˴���
	{		    
		Show_Str(30,150,240,16,"SD������!",16,0);
		delay_ms(200);				  
		Show_Str(30,170,240,16,"���չ��ܽ�������!",16,0);
		sd_ok=0;  	
	}else
	{
		Show_Str(30,150,240,16,"SD������!",16,0);
		delay_ms(200);				  
		Show_Str(30,170,240,16,"KEY_UP:����",16,0);
		sd_ok=1;  	  
	}										   						    
 	pname=mymalloc(SRAMIN,30);	//Ϊ��·�����ļ�������30���ֽڵ��ڴ�		    
 	while(pname==NULL)			//�ڴ�������
 	{	    
		Show_Str(30,190,240,16,"�ڴ����ʧ��!",16,0);
		delay_ms(200);				  
		LCD_Fill(30,190,240,146,WHITE);//�����ʾ	     
		delay_ms(200);				  
	}   											  
	while(OV7670_Init())//��ʼ��OV7670
	{
		Show_Str(30,190,240,16,"OV7670 ����!",16,0);
		delay_ms(200);
	    LCD_Fill(30,190,239,206,WHITE);
		delay_ms(200);
	}
 	Show_Str(30,190,200,16,"OV7670 ����",16,0);
	delay_ms(1500);	 		 
	TIM6_Int_Init(10000,7199);			//10Khz����Ƶ��,1�����ж�									  
	EXTI8_Init();						//ʹ�ܶ�ʱ������
	OV7670_Window_Set(12,176,240,320);	//���ô���	   ��һ
  //OV7670_Special_Effects(1);
		OV7670_CS=0;				    		    
	LCD_Clear(BLACK);
 	while(1)
	{	
		key=KEY_Scan(0);//��֧������
		if(key==KEY0_PRES)
		{
			if(sd_ok)
			{
				LED1=0;	//����DS1,��ʾ��������
				camera_new_pathname(pname);//�õ��ļ���		    
				if(bmp_encode(pname,(lcddev.width-240)/2,(lcddev.height-320)/2,120,160,0))//��������
				{
					Show_Str(40,130,240,12,"д���ļ�����!",12,0);		 
				}else 
				{
					Show_Str(40,130,240,12,"���ճɹ�!",12,0);
					Show_Str(40,150,240,12,"����Ϊ:",12,0);
 					Show_Str(40+42,150,240,12,pname,12,0);		    
 	//			BEEP=1;	//�������̽У���ʾ�������
					
					//��sd���ж�ȡ�ո������ͼƬ
					//���������ͼƬ��lcd����ʾ3s
					//������ʾ��ʾ�ַ�
					piclib_init();										//��ʼ����ͼ	 	
					LCD_Clear(BLACK);
					ai_load_picfile(pname,0,0,lcddev.width,lcddev.height,1);//��ʾͼƬ    
					Show_Str(2,2,240,16,pname,16,1); 				//��ʾͼƬ���� 
					
					if(stdbmp_send(pname))
					{	Show_Str(40,130,240,12,"�����ļ�����!",12,0);		 
					}else
					{
						Show_Str(40,130,240,12,"���ͳɹ�!",12,0);
					}
				}
				
				
			}else //��ʾSD������
			{					    
				Show_Str(40,130,240,12,"SD������!",12,0);
 				Show_Str(40,150,240,12,"���չ��ܲ�����!",12,0);			    
 			}
 		 	BEEP=0;//�رշ�����
			LED1=1;//�ر�DS1
			delay_ms(1800);//�ȴ�1.8����
			LCD_Clear(BLACK);
		}else delay_ms(5);
 		camera_refresh();//������ʾ
		i++;
		if(i==40)//DS0��˸.
		{
			i=0;
			LED0=!LED0;
 		}
	}	   										    
}













