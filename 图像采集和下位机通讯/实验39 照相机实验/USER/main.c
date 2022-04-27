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
 ALIENTEK精英STM32开发板实验39
 照相机 实验 
 技术支持：www.openedv.com
 淘宝店铺：http://eboard.taobao.com 
 关注微信公众平台微信号："正点原子"，免费获取STM32资料。
 广州市星翼电子科技有限公司  
 作者：正点原子 @ALIENTEK
************************************************/


extern u8 ov_sta;	//在exit.c里面定义
extern u8 ov_frame;	//在timer.c里面定义	   
//更新LCD显示
void camera_refresh(void)
{
	u32 j;
 	u16 color;	 
	if(ov_sta)//有帧中断更新？
	{
		LCD_Scan_Dir(U2D_L2R);		//从上到下,从左到右  
		if(lcddev.id==0X1963)LCD_Set_Window((lcddev.width-240)/2,(lcddev.height-320)/2,240,320);//将显示区域设置到屏幕中央
		else if(lcddev.id==0X5510||lcddev.id==0X5310)LCD_Set_Window((lcddev.width-320)/2,(lcddev.height-240)/2,320,240);//将显示区域设置到屏幕中央
		LCD_WriteRAM_Prepare();     //开始写入GRAM	
		OV7670_RRST=0;				//开始复位读指针 
		OV7670_RCK_L;
		OV7670_RCK_H;
		OV7670_RCK_L;
		OV7670_RRST=1;				//复位读指针结束 
		OV7670_RCK_H;
		for(j=0;j<76800;j++)
		{
			OV7670_RCK_L;
			color=GPIOC->IDR&0XFF;	//读数据
			OV7670_RCK_H; 
			color<<=8;  
			OV7670_RCK_L;
			color|=GPIOC->IDR&0XFF;	//读数据
			OV7670_RCK_H; 
			LCD->LCD_RAM=color;    
		}   							  
 		ov_sta=0;					//清零帧中断标记
		ov_frame++; 
		LCD_Scan_Dir(DFT_SCAN_DIR);	//恢复默认扫描方向 
	} 
}	   
//文件名自增（避免覆盖）
//组合成:形如"0:PHOTO/PIC13141.bmp"的文件名
void camera_new_pathname(u8 *pname)
{	 
	u8 res;					 
	u16 index=0;
	while(index<0XFFFF)
	{
		sprintf((char*)pname,"0:PHOTO/PIC%05d.bmp",index);
		res=f_open(ftemp,(const TCHAR*)pname,FA_READ);//尝试打开这个文件
		if(res==FR_NO_FILE)break;		//该文件名不存在=正是我们需要的.
		index++;
	}
}
//文件数组-像素颜色-lcd显示
u8 stdbmp_send(const u8 *filename) 
{
	FIL* f_bmp;  //性质：文件地址  指向一个文件的变量
    u16 br;		 //控制：已经读过的字节数

    u16 count;					//控制：像素增加    	   
	u8  rgb ,color_byte; 	//控制：颜色，颜色的像素颜色比
	u16 x ,y,color;	  		//控制：x，y的坐标，颜色
	u16 countpix=0;				//控制：记录像素 	 

	//控制：x,y的实际坐标	
	u16  realx=0;
	u16 realy=0;
	u8  yok=1;  				   
	u8 res;
	u16 r;  //每一个像素的red分量
	u16 g;		//green分量
	u16 b;		//blue分量
	u8 gray;

	u8 *databuf;    					//数据：数据读取存放地址
 	u16 readlen=BMP_DBUF_SIZE;//控制：一次从SD卡读取的字节数长度

	u8 *bmpbuf;			  	//性质：数据解码地址
	u8 biCompression=0;	//性质：记录压缩方式
	
	u16 rowlen;	  		 	//性质：水平方向字节数  
	BITMAPINFO *pbmp;		//临时指针 指示bitmapinfo 头部信息
	
#if BMP_USE_MALLOC == 1	//使用malloc	
	databuf=(u8*)pic_memalloc(readlen);		//开辟readlen字节的内存区域
	if(databuf==NULL)return PIC_MEM_ERR;	//内存申请失败.
	f_bmp=(FIL *)pic_memalloc(sizeof(FIL));	//开辟FIL字节的内存区域 
	if(f_bmp==NULL)							//内存申请失败.
	{		 
		pic_memfree(databuf);
		return PIC_MEM_ERR;				
	} 	 
#else				 	//不使用malloc
	databuf=bmpreadbuf;
	f_bmp=&f_bfile;
#endif
	res=f_open(f_bmp,(const TCHAR*)filename,FA_READ);//打开文件	 						  
	if(res==0)//打开成功.
	{ 
		f_read(f_bmp,databuf,readlen,(UINT*)&br);	//读出readlen个字节  
		pbmp=(BITMAPINFO*)databuf;					//得到BMP的头部信息   
		count=pbmp->bmfHeader.bfOffBits;        	//数据偏移,得到数据段的开始地址
		color_byte=pbmp->bmiHeader.biBitCount/8;	//彩色位 16/24/32  
		biCompression=pbmp->bmiHeader.biCompression;//压缩方式
		picinfo.ImgHeight=pbmp->bmiHeader.biHeight;	//得到图片高度
		picinfo.ImgWidth=pbmp->bmiHeader.biWidth;  	//得到图片宽度 
		ai_draw_init();//初始化智能画图			
		//水平像素必须是4的倍数!!
		if((picinfo.ImgWidth*color_byte)%4)rowlen=((picinfo.ImgWidth*color_byte)/4+1)*4;
		else rowlen=picinfo.ImgWidth*color_byte;
		//开始解码BMP   
		color=0;//颜色清空	 													 
		x=0 ;
		y=picinfo.ImgHeight;
		rgb=0;      
		//对于尺寸小于等于设定尺寸的图片,进行快速解码
		realy=(y*picinfo.Div_Fac)>>13;
		bmpbuf=databuf;
		while(1)
		{				 
			while(count<readlen)  //读取一簇1024扇区 (SectorsPerClust 每簇扇区数)
		    {
					if(color_byte==3)   //24位颜色图
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
				}else if(color_byte==2)  //16位颜 色图
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
				}else if(color_byte==4)//32位颜色图
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
							//alphabend=bmpbuf[count];//不读取  ALPHA通道
							break ;  		  	 
					}	
				}else if(color_byte==1)//8位色,暂时不支持,需要用到颜色表.
				{
				} 
				rgb++;	  
				count++ ;		  
				if(rgb==color_byte) //水平方向读取到1像素数数据后显示  要到所有的颜色都被读出来才进行显示
				{	
					if(x<picinfo.ImgWidth)	 					 			   
					{	
						realx=(x*picinfo.Div_Fac)>>13;//x轴实际值
						if(is_element_ok(realx,realy,1)&&yok)//符合条件
						{						 				 	  	       
							//pic_phy.draw_point(realx+picinfo.S_XOFF,realy+picinfo.S_YOFF-1,color);//显示图片	 
							r = color&0xf800;
							r = color>>11;
							g =color&0x07e0;
							g>>=5;				
							b = color&0x001f;
							//gray=r/3+g/3+b/3;
							gray=(r*30+g*59+b*11)*0.01f;
							USART_SendData(USART1, gray);//向串口1发送数据
							while(USART_GetFlagStatus(USART1,USART_FLAG_TC)!=SET);//等待发送结束
							delay_us(10);
						}   									    
					}
					x++;//x轴增加一个像素 
					color=0x0000; 
					rgb=0;  		  
				}
				countpix++;//像素累加
				
				if(countpix>=rowlen)//水平方向像素值到了.换行
				{		 
					y--; 
					if(y==0)break;			 
					realy=(y*picinfo.Div_Fac)>>13;//实际y值改变	 
					if(is_element_ok(realx,realy,0))yok=1;//此处不改变picinfo.staticx,y的值	 
					else yok=0; 
					x=0; 
					countpix=0;
					color=0x0000;
					rgb=0;
				}	 
			} 		
			res=f_read(f_bmp,databuf,readlen,(UINT *)&br);//读出readlen个字节
			if(br!=readlen)readlen=br;	//最后一批数据		  
			if(res||br==0)break;		//读取出错
			bmpbuf=databuf;
	 	 	count=0;
		}  
		f_close(f_bmp);//关闭文件
	}  	
#if BMP_USE_MALLOC == 1	//使用malloc	
	pic_memfree(databuf);	 
	pic_memfree(f_bmp);		 
#endif	
	return res;		//BMP显示结束.    					   
}	



 int main(void)
 {	 
	u8 res;							 
	u8 *pname;				//带路径的文件名 
	u8 key;					//键值		   
	u8 i;						 
	u8 sd_ok=1;				//0,sd卡不正常;1,SD卡正常.
	 
	delay_init();	    	 //延时函数初始化	  
  NVIC_PriorityGroupConfig(NVIC_PriorityGroup_2);//设置中断优先级分组为组2：2位抢占优先级，2位响应优先级
	uart_init(115200);	 	//串口初始化为115200
 	usmart_dev.init(72);		//初始化USMART		
 	LED_Init();		  			//初始化与LED连接的硬件接口
	KEY_Init();					//初始化按键
	LCD_Init();			   		//初始化LCD    
	BEEP_Init();        		//蜂鸣器初始化	 
	W25QXX_Init();				//初始化W25Q128
 	my_mem_init(SRAMIN);		//初始化内部内存池
	exfuns_init();				//为fatfs相关变量申请内存  
 	f_mount(fs[0],"0:",1); 		//挂载SD卡 
 	f_mount(fs[1],"1:",1); 		//挂载FLASH. 
	POINT_COLOR=RED;      
 	while(font_init()) 				//检查字库
	{	    
		LCD_ShowString(30,50,200,16,16,"Font Error!");
		delay_ms(200);				  
		LCD_Fill(30,50,240,66,WHITE);//清除显示	     
	}  	 
 	Show_Str(30,50,200,16,"精英STM32F1开发板",16,0);				    	 
	Show_Str(30,70,200,16,"照相机实验",16,0);				    	 
	Show_Str(30,90,200,16,"KEY0:拍照",16,0);				    	 
	res=f_mkdir("0:/PHOTO");		//创建PHOTO文件夹
	if(res!=FR_EXIST&&res!=FR_OK) 	//发生了错误
	{		    
		Show_Str(30,150,240,16,"SD卡错误!",16,0);
		delay_ms(200);				  
		Show_Str(30,170,240,16,"拍照功能将不可用!",16,0);
		sd_ok=0;  	
	}else
	{
		Show_Str(30,150,240,16,"SD卡正常!",16,0);
		delay_ms(200);				  
		Show_Str(30,170,240,16,"KEY_UP:拍照",16,0);
		sd_ok=1;  	  
	}										   						    
 	pname=mymalloc(SRAMIN,30);	//为带路径的文件名分配30个字节的内存		    
 	while(pname==NULL)			//内存分配出错
 	{	    
		Show_Str(30,190,240,16,"内存分配失败!",16,0);
		delay_ms(200);				  
		LCD_Fill(30,190,240,146,WHITE);//清除显示	     
		delay_ms(200);				  
	}   											  
	while(OV7670_Init())//初始化OV7670
	{
		Show_Str(30,190,240,16,"OV7670 错误!",16,0);
		delay_ms(200);
	    LCD_Fill(30,190,239,206,WHITE);
		delay_ms(200);
	}
 	Show_Str(30,190,200,16,"OV7670 正常",16,0);
	delay_ms(1500);	 		 
	TIM6_Int_Init(10000,7199);			//10Khz计数频率,1秒钟中断									  
	EXTI8_Init();						//使能定时器捕获
	OV7670_Window_Set(12,176,240,320);	//设置窗口	   改一
  //OV7670_Special_Effects(1);
		OV7670_CS=0;				    		    
	LCD_Clear(BLACK);
 	while(1)
	{	
		key=KEY_Scan(0);//不支持连按
		if(key==KEY0_PRES)
		{
			if(sd_ok)
			{
				LED1=0;	//点亮DS1,提示正在拍照
				camera_new_pathname(pname);//得到文件名		    
				if(bmp_encode(pname,(lcddev.width-240)/2,(lcddev.height-320)/2,120,160,0))//拍照有误
				{
					Show_Str(40,130,240,12,"写入文件错误!",12,0);		 
				}else 
				{
					Show_Str(40,130,240,12,"拍照成功!",12,0);
					Show_Str(40,150,240,12,"保存为:",12,0);
 					Show_Str(40+42,150,240,12,pname,12,0);		    
 	//			BEEP=1;	//蜂鸣器短叫，提示拍照完成
					
					//从sd卡中读取刚刚拍摄的图片
					//将刚拍摄的图片在lcd上显示3s
					//并且提示显示字符
					piclib_init();										//初始化画图	 	
					LCD_Clear(BLACK);
					ai_load_picfile(pname,0,0,lcddev.width,lcddev.height,1);//显示图片    
					Show_Str(2,2,240,16,pname,16,1); 				//显示图片名字 
					
					if(stdbmp_send(pname))
					{	Show_Str(40,130,240,12,"发送文件错误!",12,0);		 
					}else
					{
						Show_Str(40,130,240,12,"发送成功!",12,0);
					}
				}
				
				
			}else //提示SD卡错误
			{					    
				Show_Str(40,130,240,12,"SD卡错误!",12,0);
 				Show_Str(40,150,240,12,"拍照功能不可用!",12,0);			    
 			}
 		 	BEEP=0;//关闭蜂鸣器
			LED1=1;//关闭DS1
			delay_ms(1800);//等待1.8秒钟
			LCD_Clear(BLACK);
		}else delay_ms(5);
 		camera_refresh();//更新显示
		i++;
		if(i==40)//DS0闪烁.
		{
			i=0;
			LED0=!LED0;
 		}
	}	   										    
}













