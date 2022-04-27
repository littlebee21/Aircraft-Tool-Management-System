using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;
using ZXing.Common;
using ZXing;
using System.Drawing.Imaging;
using System.IO.Ports;
using System.IO;
using Excel = Microsoft.Office.Interop.Excel;
using System.Diagnostics;





namespace E01_条形码
{
    public partial class timebox1 : Form
    {
        Image _img;
        //修改1
        public static SerialPort sp = null;
        bool isOpen = false;
        bool isSetProperty = false;
        bool isHex = false;
        Bitmap OvImage = new Bitmap(240, 320);
        static int value = 193;
        static Bitmap bm;
        //修改2
        public static byte[] ReceivedData;
        Stopwatch stopwatch = new Stopwatch();
        Stopwatch stopwatch2 = new Stopwatch();




        public timebox1()
        {
            InitializeComponent();
            Initialize();

        }

        private bool sourceAvailable;
        OpenFileDialog fileDialog = new OpenFileDialog();


        private void Initialize()
        {
            sourceAvailable = false;


            fileDialog.Filter = "图片文件|*.bmp;*.jpg;*.png;*.ico";
            fileDialog.FilterIndex = 1;
            fileDialog.RestoreDirectory = true;
        }



        private void Form1_Load(object sender, EventArgs e)
        {
            timer1.Enabled = true;
            timer1.Interval = 1000;
            this.label3.Text = Convert.ToString(DateTime.Now.ToLocalTime());
            timer2.Interval = 1000;
            timer2.Start();
            


            this.MaximumSize = this.Size;
            this.MinimumSize = this.Size;
            this.MaximizeBox = false;
            for (int i = 0; i < 30; i++)//最大支持到串口10，可根据自己需求增加
            {
                cbbCOMPort.Items.Add("COM" + (i + 1).ToString());
            }
            cbbCOMPort.SelectedIndex = 0;

            cbbBaudRate.Items.Add("1200");
            cbbBaudRate.Items.Add("2400");
            cbbBaudRate.Items.Add("4800");
            cbbBaudRate.Items.Add("9600");
            cbbBaudRate.Items.Add("19200");
            cbbBaudRate.Items.Add("38400");
            cbbBaudRate.Items.Add("43000");
            cbbBaudRate.Items.Add("56000");
            cbbBaudRate.Items.Add("57600");
            cbbBaudRate.Items.Add("115200");
            cbbBaudRate.Items.Add("230400");
            cbbBaudRate.Items.Add("256000");
            cbbBaudRate.Items.Add("1500000");
            cbbBaudRate.SelectedIndex = 9;

            cbbStopBits.Items.Add("0");
            cbbStopBits.Items.Add("1");
            cbbStopBits.Items.Add("1.5");
            cbbStopBits.Items.Add("2");
            cbbStopBits.SelectedIndex = 1;

            cbbDataBits.Items.Add("8");
            cbbDataBits.Items.Add("7");
            cbbDataBits.Items.Add("6");
            cbbDataBits.Items.Add("5");
            cbbDataBits.SelectedIndex = 0;

            cbbParity.Items.Add("无");
            cbbParity.Items.Add("奇校验");
            cbbParity.Items.Add("偶校验");
            cbbParity.SelectedIndex = 0;

            rbtnHex.Checked = true;

            //初始接收字符数目为0
            tbxRecvLength.Text = "0";
          }




        // （1） 二维码识别模块 

        // 生成一维码
        private void Create1DBtn_Click(object sender, EventArgs e)
        {
            // 1.设置条形码规格
            EncodingOptions encodeOption = new EncodingOptions();
            encodeOption.Height = 180; // 必须制定高度、宽度
            encodeOption.Width = 300;

            // 2.生成条形码图片并保存
            ZXing.BarcodeWriter wr = new BarcodeWriter();
            wr.Options = encodeOption;
            wr.Format = BarcodeFormat.CODE_39; //  条形码规格：EAN13规格：12（无校验位）或13位数字
            Bitmap img = wr.Write(this.ContentTxt.Text); // 生成图片
            barCodeImg.Image = img;
            _img = img;

        }

        // 生成二维码
        private void Create2DBtn_Click(object sender, EventArgs e)
        {
            // 1.设置QR二维码的规格
            ZXing.QrCode.QrCodeEncodingOptions qrEncodeOption = new ZXing.QrCode.QrCodeEncodingOptions();
            qrEncodeOption.CharacterSet = "UTF-8"; // 设置编码格式，否则读取'中文'乱码
            qrEncodeOption.Height = 220;
            qrEncodeOption.Width = 220;
            qrEncodeOption.Margin = 1; // 设置周围空白边距

            // 2.生成条形码图片并保存
            ZXing.BarcodeWriter wr = new BarcodeWriter();
            wr.Format = BarcodeFormat.QR_CODE; // 二维码
            wr.Options = qrEncodeOption;//二维码的规格
            Bitmap img = wr.Write(this.ContentTxt.Text);
            barCodeImg.Image = img;
            _img = img;

        }


        // 读取一维码
        private void Read1DBtn_Click(object sender, EventArgs e)
        {
            // 1.设置读取条形码规格
            DecodingOptions decodeOption = new DecodingOptions();
            decodeOption.PossibleFormats = new List<BarcodeFormat>() {
               BarcodeFormat.EAN_13,
            };

            // 2.进行读取操作
            ZXing.BarcodeReader br = new BarcodeReader();
            br.Options = decodeOption;
            ZXing.Result rs = br.Decode(this.barCodeImg.Image as Bitmap);
            if (rs == null)
            {
                this.ContentTxt.Text = "读取失败";
                MessageBox.Show("读取失败");
            }
            else
            {
                this.ContentTxt.Text = rs.Text;
                MessageBox.Show("读取成功，内容：" + rs.Text);
            }
        }

        // 读取二维码
        private void Read2DBtn_Click(object sender, EventArgs e)
        {
           // 1.设置读取条形码规格
            DecodingOptions decodeOption = new DecodingOptions();
            decodeOption.PossibleFormats = new List<BarcodeFormat>() {
               BarcodeFormat.QR_CODE,
           };

            // 2.进行读取操作
            ZXing.BarcodeReader br = new BarcodeReader();
            br.Options = decodeOption;
            ZXing.Result rs = br.Decode(this.barCodeImg.Image as Bitmap);
            if (rs == null)
            {
                this.ContentTxt.Text = "读取失败";
                DialogResult res = MessageBox.Show("读取失败，是否再次尝试？", "提示", MessageBoxButtons.OKCancel);
                if (res == DialogResult.OK)
                {

                    Bitmap sourceImage = new Bitmap(barCodeImg.Image);
                    Bitmap bmp = new Bitmap(sourceImage);
                    int[] sharpT = { -1, -1, -1, -1, 9, -1, -1, -1, -1 };  //拉普拉斯高增滤波模板
                    Bitmap sharpPicture = SmoothSharp(bmp, sharpT, 1);
                    barCodeImg.Image = (Image)sharpPicture;
                }
                else
                {
                    //
                }
            }
            else
            {
                this.ContentTxt.Text = rs.Text;
                //this.textBox1.Text = rs.Text;
               

                MessageBox.Show("读取成功，内容：" + rs.Text);
                //string[] arr = new string[ContentTxt.Lines.Length];
                //for (int i = 0; i < ContentTxt.Lines.Length; i++)
                //{
                //    arr[i] = ContentTxt.Lines[i];
                //}
                //float code=0;
                //int si = 0;
                //foreach (string i in arr)
                //{
                //    Console.WriteLine(i.ToString());
                //    if (si == 1)
                //        code = float.Parse(i);
                   
                //    si++;
                //}
               
                
                
                   
                
            }
        }

        // 打开图片
        private void OpenImgBtn_Click(object sender, EventArgs e)
        {
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                string fileName = fileDialog.FileName;
                barCodeImg.Image = Image.FromFile(fileName);

                
            }
        }



        //保存图片
        private void save_Click(object sender, EventArgs e)
        {

            Image img = (Image)barCodeImg.Image.Clone();
            using (Brush brush = new SolidBrush(label1.ForeColor))
            using (Graphics g = Graphics.FromImage(img))
            {
                Rectangle rect = new Rectangle(label1.Left - barCodeImg.Left, label1.Top - barCodeImg.Top, label1.Width, label1.Height);
                if (label1.BackColor != Color.Transparent)
                {
                    using (Brush bgBrush = new SolidBrush(label1.BackColor))
                    {
                        g.FillRectangle(bgBrush, rect);
                    }
                }
                //g.DrawString(label1.Text, label1.Font, brush, rect, StringFormat.GenericDefault);
                g.Save();
            }
            img.Save("d:\\123.png", System.Drawing.Imaging.ImageFormat.Png);
        }
        private void barCodeImg_Click(object sender, EventArgs e)
        {

        }

        private void ImgPathTxt_TextChanged(object sender, EventArgs e)
        {

        }
         
         
       
        //显示信息
        private void button3_Click(object sender, EventArgs e)
        {
            string str = @"Data Source=;Initial catalog=feiji;integrated Security=True";
            SqlConnection conn = new SqlConnection(str);
            conn.Open();

            SqlDataAdapter sqlDap = new SqlDataAdapter("Select * from gongju", conn);
            DataSet dds = new DataSet();
            sqlDap.Fill(dds);
            DataTable _table = dds.Tables[0];
            int count = _table.Rows.Count;
            dataGridView1.DataSource = _table;
            //dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            dataGridView1.RowsDefaultCellStyle.BackColor = Color.Bisque;
            dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Color.Beige;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCellsExceptHeader;
            conn.Close();

        }
        //显示行号
        private void dataGridView1_RowPostPaint_1(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            System.Drawing.Rectangle rectangle = new System.Drawing.Rectangle(e.RowBounds.Location.X,
            e.RowBounds.Location.Y,
            dataGridView1.RowHeadersWidth - 4,
            e.RowBounds.Height);
            TextRenderer.DrawText(e.Graphics, (e.RowIndex + 1).ToString(),
            dataGridView1.RowHeadersDefaultCellStyle.Font,
            rectangle,
            dataGridView1.RowHeadersDefaultCellStyle.ForeColor,
            TextFormatFlags.VerticalCenter | TextFormatFlags.Right);

        }

 
       
        //名称查询
        private void button5_Click_1(object sender, EventArgs e)
        {
             if (this.textBox2.Text.Length > 0)
            {
                string connString = @"Data Source=;Initial catalog=feiji;integrated Security=True";
                //string connString = "Data Source=DELL-PC;Initial Catalog=airplane;Persist Security Info=True;User ID=sa;Password=1";
                string sql = string.Format("Select  * from gongju where 工具名称='{0}'", this.textBox2.Text);
                try
                {
                    SqlConnection connection = new SqlConnection(connString);
                    connection.Open();
                    SqlCommand myCommand = new SqlCommand(sql, connection);
                    SqlDataReader dr = myCommand.ExecuteReader();
                    if (dr.HasRows)
                    {
                        this.label15.Text = "工具名称有效";//dr["要显示的列名"].ToString();
                        SqlDataAdapter adpater = new SqlDataAdapter(sql, connString);
                        DataSet dataSet = new DataSet();
                        adpater.Fill(dataSet, "gongju");
                        dataGridView1.DataSource = dataSet.Tables["gongju"]; //把容器放到表
                        dataGridView1.RowsDefaultCellStyle.BackColor = Color.Bisque;
                        dataGridView1.AlternatingRowsDefaultCellStyle.BackColor = Color.Beige;
                        dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCellsExceptHeader;
                        connection.Close();
                    }

                    else
                    {
                        this.label15.Text = "该名称不存在";
                    }

                }
                catch (SqlException ex)
                {
                    this.label15.Text = ex.Message;
                }

            }
            else
            {
                this.label15.Text = "请输入名称!";
            }

        }
        //出厂编号查询
        private void button8_Click(object sender, EventArgs e)
        {
            if (this.textBox3.Text.Length > 0)
            {
                string connString = @"Data Source=;Initial catalog=feiji;integrated Security=True";
                //string connString = "Data Source=DELL-PC;Initial Catalog=airplane;Persist Security Info=True;User ID=sa;Password=1";
                string sql = string.Format("Select top 1 * from gongju where 出厂编号='{0}'", this.textBox3.Text);
                try
                {
                    SqlConnection connection = new SqlConnection(connString);
                    connection.Open();
                    SqlCommand myCommand = new SqlCommand(sql, connection);
                    SqlDataReader dr = myCommand.ExecuteReader();
                    if (dr.HasRows)
                    {
                        this.label18.Text = "编号有效";//dr["要显示的列名"].ToString();
                        SqlDataAdapter adpater = new SqlDataAdapter(sql, connString);
                        DataSet dataSet = new DataSet();
                        adpater.Fill(dataSet, "gongju");
                        dataGridView1.DataSource = dataSet.Tables["gongju"]; //把容器放到表
                        dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCellsExceptHeader;
                        connection.Close();
                    }

                    else
                    {
                        this.label18.Text = "该编号不存在";
                    }

                }
                catch (SqlException ex)
                {
                    this.label18.Text = ex.Message;
                }

            }
            else
            {
                this.label18.Text = "请输入编号!";
            }
        }

        
        //入库
        private void button6_Click(object sender, EventArgs e)
        {
            string str = @"Data Source=;Initial Catalog=feiji;Integrated Security=True";
            SqlConnection conn = new SqlConnection(str);
            conn.Open();
            string selectsql = "update gongju set 状态 = 'IN' ,备注信息= '" + textBox5.Text + "',更改时间= '" + textBox4.Text + "' where 工具名称= '" + textBox1.Text + "'";
            SqlCommand cmd = new SqlCommand(selectsql, conn);
            SqlDataReader sdr = cmd.ExecuteReader();
            if (sdr != null)
            {
                MessageBox.Show("成功！！");
                sdr.Read();
            }
            else
            {
                MessageBox.Show("请重新输入！！");
            }
            //UpdataForm udform2 = new UpdataForm();
            //this.DialogResult = System.Windows.Forms.DialogResult.Yes;

            conn.Close();

        }
        //出库
        private void button7_Click(object sender, EventArgs e)
        {
            SqlConnection sqlconnect = new SqlConnection("Data Source=;Initial Catalog=feiji;Integrated Security=True");
            sqlconnect.Open();
            string str = "update gongju set 状态 = 'OUT' ,备注信息= '" + textBox5.Text + "',更改时间= '" + textBox4.Text + "' where 工具名称='" + textBox1.Text + "'";
            SqlCommand sqlcommand = new SqlCommand(str, sqlconnect);
            SqlDataReader dateReader = sqlcommand.ExecuteReader();
            if (dateReader != null)
            {
                MessageBox.Show("查找成功！！");
                dateReader.Read();
            }
            else
            {
                MessageBox.Show("请重新输入！！");
            }
            sqlconnect.Close();
        }
        //时间设定
        private void button9_Click(object sender, EventArgs e)
        {
            string str = @"Data Source=DELL-PC;Initial catalog=airplane;integrated Security=True";
            SqlConnection conn = new SqlConnection(str);
            conn.Open();
            string selectsql = "update 杨杰 set 备注 = '" + textBox4.Text+ "' where 编号 = '" + textBox2.Text + "'";
            SqlCommand cmd = new SqlCommand(selectsql, conn);
            cmd.CommandType = CommandType.Text;
            SqlDataReader sdr;
            sdr = cmd.ExecuteReader();
            //UpdataForm udform2 = new UpdataForm();
            this.DialogResult = System.Windows.Forms.DialogResult.Yes;

            conn.Close();
        }
        //入库/出库时间
        private void dataGridView1_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.ColumnIndex == 7 && e.RowIndex >= 0)
            {
                dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value = DateTime.Now;
            }
        }
        //数据库显示翻页

        private void btnNEXTpage_Click(object sender, EventArgs e)
        {
            if (dataGridView1.Rows.Count - dataGridView1.CurrentCell.RowIndex > 15)
            {
                dataGridView1.CurrentCell = dataGridView1[0, dataGridView1.CurrentCell.RowIndex + 15];
            }
            else if (dataGridView1.Rows.Count - dataGridView1.CurrentCell.RowIndex > 1)
            {
                dataGridView1.CurrentCell = dataGridView1[0, dataGridView1.Rows.Count - 1];
            }
            else
            {
                MessageBox.Show("当前已是最后一页", "提示");
            }
        }

        private void btnLASTpage_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentCell.RowIndex > 14)
            {
                dataGridView1.CurrentCell = dataGridView1[0, dataGridView1.CurrentCell.RowIndex - 15];
            }
            else if (dataGridView1.CurrentCell.RowIndex <= 14 && dataGridView1.CurrentCell.RowIndex != 0)
            {
                dataGridView1.CurrentCell = dataGridView1[0, 0];
            }
            else
            {
                MessageBox.Show("当前已是第一页", "提示");
            }
        }
        private void btnNextRow_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentCell.RowIndex < dataGridView1.Rows.Count - 1)
            {
                dataGridView1.CurrentCell = dataGridView1[0, dataGridView1.CurrentCell.RowIndex + 1];
            }
            else
            {
                MessageBox.Show("当前已是最后一行", "提示");
            }
        }

        private void btnLastRow_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentCell.RowIndex != 0)
            {
                dataGridView1.CurrentCell = dataGridView1[0, dataGridView1.CurrentCell.RowIndex - 1];
            }
            else
            {
                MessageBox.Show("当前已是第一行", "提示");
            }
        }
        //数据导入到EXCEL
        private void btnExport_Click(object sender, EventArgs e)
        {
            string fileName = "";
            string saveFileName = "";
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.DefaultExt = "xlsx";
            saveDialog.Filter = "Excel文件|*.xlsx";
            saveDialog.FileName = fileName;
            saveDialog.ShowDialog();
            saveFileName = saveDialog.FileName;
            if (saveFileName.IndexOf(":") < 0) return; //被点了取消
            Microsoft.Office.Interop.Excel.Application xlApp = 
                                new Microsoft.Office.Interop.Excel.Application();
            if (xlApp == null)
            {
                MessageBox.Show("无法创建Excel对象，您的电脑可能未安装Excel");
                return;
            }
            Microsoft.Office.Interop.Excel.Workbooks workbooks = xlApp.Workbooks;
            Microsoft.Office.Interop.Excel.Workbook workbook =
                        workbooks.Add(Microsoft.Office.Interop.Excel.XlWBATemplate.xlWBATWorksheet);
            Microsoft.Office.Interop.Excel.Worksheet worksheet =
                        (Microsoft.Office.Interop.Excel.Worksheet)workbook.Worksheets[1];//取得sheet1 
            //写入标题             
            for (int i = 0; i < dataGridView1.ColumnCount; i++)
            { worksheet.Cells[1, i + 1] = dataGridView1.Columns[i].HeaderText; }
            //写入数值
            for (int r = 0; r < dataGridView1.Rows.Count; r++)
            {
                for (int i = 0; i < dataGridView1.ColumnCount; i++)
                {
                    worksheet.Cells[r + 2, i + 1] = dataGridView1.Rows[r].Cells[i].Value;
                }
                System.Windows.Forms.Application.DoEvents();
            }
            worksheet.Columns.EntireColumn.AutoFit();//列宽自适应
            MessageBox.Show(fileName + "资料保存成功", "提示", MessageBoxButtons.OK);
            if (saveFileName != "")
            {
                try
                {
                    workbook.Saved = true;
                    workbook.SaveCopyAs(saveFileName);  //fileSaved = true;                 
                }
                catch (Exception ex)
                {//fileSaved = false;                      
                    MessageBox.Show("导出文件时出错,文件可能正被打开！\n" + ex.Message);
                }
            }
            xlApp.Quit();
            GC.Collect();//强行销毁

        }
       

       

      

        


      

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            this.label3.Text = Convert.ToString(DateTime.Now.ToLocalTime());
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void label_Source_Click(object sender, EventArgs e)
        {

        }



        // （2） 图像处理模块

        public class RGB
        {
            public const short R = 2;
            public const short G = 1;
            public const short B = 0;

            public byte Red;
            public byte Green;
            public byte Blue;

            public System.Drawing.Color Color
            {
                         get { return Color.FromArgb(Red, Green, Blue); }
                set
                {
                    Red = value.R;
                    Green = value.G;
                    Blue = value.B;
                }
            }

            public RGB() { }

            public RGB(byte red, byte green, byte blue)
            {
                this.Red = red;
                this.Green = green;
                this.Blue = blue;
            }

            public RGB(System.Drawing.Color color)
            {
                this.Red = color.R;
                this.Green = color.G;
                this.Blue = color.B;
            }
        }


        private void button_LoadImage_Click(object sender, EventArgs e)
        {
            
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //if (!sourceAvailable)
           // {
            //    MessageBox.Show("Please load source image first!");
            //    return;
           // }

            Bitmap sourceImage = new Bitmap(barCodeImg.Image);
            FormatImage(ref sourceImage);

            Bitmap destImage = getTargetImage(sourceImage, true);


            if (sourceImage != null)
            {
                destImage = sourceImage.Clone() as Bitmap;

                Color pixel;
                int ret;
                for (int x = 0; x < destImage.Width; x++)
                {
                    for (int y = 0; y < destImage.Height; y++)
                    {
                        pixel = destImage.GetPixel(x, y);
                        ret = (int)(pixel.R * 0.299 + pixel.G * 0.587 + pixel.B * 0.114);
                        destImage.SetPixel(x, y, Color.FromArgb(ret, ret, ret));
                    }
                }
                barCodeImg.Image = destImage.Clone() as Image;
            }
        }

        public static Bitmap ConvertTo1Bpp1(Bitmap bmp)
        {
            int average = 0;
            for (int i = 0; i < bmp.Width; i++)
            {
                for (int j = 0; j < bmp.Height; j++)
                {
                    Color color = bmp.GetPixel(i, j);
                    average += color.B;
                }
            }
            average = (int)average / (bmp.Width * bmp.Height);

            for (int i = 0; i < bmp.Width; i++)
            {
                for (int j = 0; j < bmp.Height; j++)
                {
                    //获取该点的像素的RGB的颜色
                    Color color = bmp.GetPixel(i, j);
                    int value = 255 - color.B;
                    Color newColor = (value > average ? Color.FromArgb(0, 0, 0) : Color.FromArgb(255, 255, 255));
                    bmp.SetPixel(i, j, newColor);
                }
            }
            return bmp;
        }

        private void btnDouble_Click(object sender, EventArgs e)
        {
            Bitmap sourceImage = new Bitmap(barCodeImg.Image);
            Bitmap destImage = ConvertTo1Bpp1(sourceImage);
            barCodeImg.Image = destImage;//.Clone() as Image;
        }


        private void button_Med_Click(object sender, EventArgs e)
        {
           // if (!sourceAvailable)
           // {
           //     MessageBox.Show("Please load source image first!");
           //     return;
           //}

            Bitmap sourceImage = new Bitmap(barCodeImg.Image);
            FormatImage(ref sourceImage);

            Bitmap destImage = getTargetImage(sourceImage, true);

            barCodeImg.Image = destImage;
        }

        private void FormatImage(ref Bitmap image)
        {
            if (
                (image.PixelFormat != PixelFormat.Format24bppRgb) &&
                (image.PixelFormat != PixelFormat.Format8bppIndexed)
                )
            {
                image = image.Clone(new Rectangle(0, 0, image.Width, image.Height), PixelFormat.Format24bppRgb);
            }
        }

        private Bitmap getTargetImage(Bitmap sourceImage, bool isMedian)
        {
            // get image dimension
            int width = sourceImage.Width;
            int height = sourceImage.Height;
            PixelFormat format = sourceImage.PixelFormat;

            // lock source bitmap data
            BitmapData sourceData = sourceImage.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadOnly,
                format);

            // create new image
            Bitmap destImage = new Bitmap(width, height, format);

            // lock destination bitmap data
            BitmapData destData = destImage.LockBits(
                new Rectangle(0, 0, width, height),
                ImageLockMode.ReadWrite,
                format);

            // process the filter
            if (isMedian)
            {
                ProcessFilter_Median(sourceData, destData);
            }
            else
            {
                ProcessFilter(sourceData, destData);
            }

            // unlock destination images
            destImage.UnlockBits(destData);

            // unlock source image
            sourceImage.UnlockBits(sourceData);

            return destImage;
        }

        private int[,] kernel;
        private int size;
        private unsafe void ProcessFilter(BitmapData sourceData, BitmapData destData)
        {
            size = kernel.GetLength(0);

            // get source image size
            int width = sourceData.Width;
            int height = sourceData.Height;

            int stride = sourceData.Stride;
            int offset = stride - ((sourceData.PixelFormat == PixelFormat.Format8bppIndexed) ? width : width * 3);

            // loop and array indexes
            int i, j, t, k, ir, jr;
            // kernel's radius
            int radius = size >> 1;
            // color sums
            long r, g, b, div;

            byte* src = (byte*)sourceData.Scan0.ToPointer();
            byte* dst = (byte*)destData.Scan0.ToPointer();
            byte* p;

            // do the processing job
            if (sourceData.PixelFormat == PixelFormat.Format8bppIndexed)
            {
                // grayscale image

                // for each line
                for (int y = 0; y < height; y++)
                {
                    // for each pixel
                    for (int x = 0; x < width; x++, src++, dst++)
                    {
                        g = div = 0;

                        // for each kernel row
                        for (i = 0; i < size; i++)
                        {
                            ir = i - radius;
                            t = y + ir;

                            // skip row
                            if (t < 0)
                                continue;
                            // break
                            if (t >= height)
                                break;

                            // for each kernel column
                            for (j = 0; j < size; j++)
                            {
                                jr = j - radius;
                                t = x + jr;

                                // skip column
                                if (t < 0)
                                    continue;

                                if (t < width)
                                {
                                    k = kernel[i, j];

                                    div += k;
                                    g += k * src[ir * stride + jr];
                                }
                            }
                        }

                        // check divider
                        if (div != 0)
                        {
                            g /= div;
                        }
                        *dst = (g > 255) ? (byte)255 : ((g < 0) ? (byte)0 : (byte)g);
                    }
                    src += offset;
                    dst += offset;
                }
            }
            else
            {
                // RGB image

                // for each line
                for (int y = 0; y < height; y++)
                {
                    // for each pixel
                    for (int x = 0; x < width; x++, src += 3, dst += 3)
                    {
                        r = g = b = div = 0;

                        // for each kernel row
                        for (i = 0; i < size; i++)
                        {
                            ir = i - radius;
                            t = y + ir;

                            // skip row
                            if (t < 0)
                                continue;
                            // break
                            if (t >= height)
                                break;

                            // for each kernel column
                            for (j = 0; j < size; j++)
                            {
                                jr = j - radius;
                                t = x + jr;

                                // skip column
                                if (t < 0)
                                    continue;

                                if (t < width)
                                {
                                    k = kernel[i, j];
                                    p = &src[ir * stride + jr * 3];

                                    div += k;

                                    r += k * p[RGB.R];
                                    g += k * p[RGB.G];
                                    b += k * p[RGB.B];
                                }
                            }
                        }

                        // check divider
                        if (div != 0)
                        {
                            r /= div;
                            g /= div;
                            b /= div;
                        }
                        dst[RGB.R] = (r > 255) ? (byte)255 : ((r < 0) ? (byte)0 : (byte)r);
                        dst[RGB.G] = (g > 255) ? (byte)255 : ((g < 0) ? (byte)0 : (byte)g);
                        dst[RGB.B] = (b > 255) ? (byte)255 : ((b < 0) ? (byte)0 : (byte)b);
                    }
                    src += offset;
                    dst += offset;
                }
            }
        }

        private unsafe void ProcessFilter_Median(BitmapData sourceData, BitmapData destData)
        {
            size = 3;

            // get source image size
            int width = sourceData.Width;
            int height = sourceData.Height;

            int stride = sourceData.Stride;
            int offset = stride - ((sourceData.PixelFormat == PixelFormat.Format8bppIndexed) ? width : width * 3);

            // loop and array indexes
            int i, j, t;
            // processing square's radius
            int radius = size >> 1;
            // number of elements
            int c;

            // array to hold pixel values (R, G, B)
            byte[] r = new byte[size * size];
            byte[] g = new byte[size * size];
            byte[] b = new byte[size * size];

            byte* src = (byte*)sourceData.Scan0.ToPointer();
            byte* dst = (byte*)destData.Scan0.ToPointer();
            byte* p;

            // do the processing job
            if (sourceData.PixelFormat == PixelFormat.Format8bppIndexed)
            {
                // grayscale image

                // for each line
                for (int y = 0; y < height; y++)
                {
                    // for each pixel
                    for (int x = 0; x < width; x++, src++, dst++)
                    {
                        c = 0;

                        // for each kernel row
                        for (i = -radius; i <= radius; i++)
                        {
                            t = y + i;

                            // skip row
                            if (t < 0)
                                continue;
                            // break
                            if (t >= height)
                                break;

                            // for each kernel column
                            for (j = -radius; j <= radius; j++)
                            {
                                t = x + j;

                                // skip column
                                if (t < 0)
                                    continue;

                                if (t < width)
                                {
                                    g[c++] = src[i * stride + j];
                                }
                            }
                        }
                        // sort elements
                        Array.Sort(g, 0, c);
                        // get the median
                        *dst = g[c >> 1];
                    }
                    src += offset;
                    dst += offset;
                }
            }
            else
            {
                // RGB image

                // for each line
                for (int y = 0; y < height; y++)
                {
                    // for each pixel
                    for (int x = 0; x < width; x++, src += 3, dst += 3)
                    {
                        c = 0;

                        // for each kernel row
                        for (i = -radius; i <= radius; i++)
                        {
                            t = y + i;

                            // skip row
                            if (t < 0)
                                continue;
                            // break
                            if (t >= height)
                                break;

                            // for each kernel column
                            for (j = -radius; j <= radius; j++)
                            {
                                t = x + j;

                                // skip column
                                if (t < 0)
                                    continue;

                                if (t < width)
                                {
                                    p = &src[i * stride + j * 3];

                                    r[c] = p[RGB.R];
                                    g[c] = p[RGB.G];
                                    b[c] = p[RGB.B];
                                    c++;
                                }
                            }
                        }

                        // sort elements
                        Array.Sort(r, 0, c);
                        Array.Sort(g, 0, c);
                        Array.Sort(b, 0, c);
                        // get the median
                        t = c >> 1;
                        dst[RGB.R] = r[t];
                        dst[RGB.G] = g[t];
                        dst[RGB.B] = b[t];
                    }
                    src += offset;
                    dst += offset;
                }
            }
        }

        private void button_Sharpen_Click(object sender, EventArgs e)
        {
          

            Bitmap sourceImage = new Bitmap(barCodeImg.Image);
            Bitmap bmp = new Bitmap(sourceImage);
            int[] sharpT = { -1, -1, -1, -1, 9, -1, -1, -1, -1 };  //拉普拉斯高增滤波模板
            Bitmap sharpPicture = SmoothSharp(bmp, sharpT, 1);
            barCodeImg.Image = (Image)sharpPicture;
        }

        private static Bitmap SmoothSharp(Bitmap oldPicture, int[] smoothT, int smoothC)
        {
            int width = oldPicture.Width;       //图片的宽度，以像素为单位
            int height = oldPicture.Height;    //图片的高度，以像素为单位
            Bitmap newPicture = new Bitmap(width, height);       //处理后的新图
            int[] template = smoothT;    //将3*3的卷积模板存入一维数组中
            int count = smoothC;     //模板数组要除的整数
            Color pixel;



            //图像边缘不作处理
            for (int i = 0; i < width; i++) newPicture.SetPixel(i, 0, oldPicture.GetPixel(i, 0));
            for (int i = 0; i < width; i++) newPicture.SetPixel(i, height - 1, oldPicture.GetPixel(i, height - 1));
            for (int i = 0; i < height; i++) newPicture.SetPixel(0, i, oldPicture.GetPixel(0, i));
            for (int i = 0; i < height; i++) newPicture.SetPixel(width - 1, i, oldPicture.GetPixel(width - 1, i));

            //卷积运算,数组横向扫描
            for (int line = 1; line < height - 1; line++)
            {
                for (int col = 1; col < width - 1; col++)
                {
                    int r = 0, g = 0, b = 0, index = 0;
                    //3*3模板，加权求和
                    for (int l = -1; l < 2; l++)
                    {
                        for (int c = -1; c < 2; c++)
                        {
                            pixel = oldPicture.GetPixel(col + c, line + l);
                            r += pixel.R * template[index];
                            g += pixel.G * template[index];
                            b += pixel.B * template[index];
                            index++;
                        }
                    }
                    r /= count; g /= count; b /= count;
                    //溢出的判断，不能反色
                    r = (r > 255 ? 255 : r);
                    r = (r < 0 ? 0 : r);
                    g = (g > 255 ? 255 : g);
                    g = (g < 0 ? 0 : g);
                    b = (b > 255 ? 255 : b);
                    b = (b < 0 ? 0 : b);
                    newPicture.SetPixel(col, line, Color.FromArgb(r, g, b));
                }
            }
            return newPicture;
        }




        private void button5_Click(object sender, EventArgs e)
        {
            Image img = (Image)barCodeImg.Image.Clone();
            using (Brush brush = new SolidBrush(label1.ForeColor))
            using (Graphics g = Graphics.FromImage(img))
            {
                Rectangle rect = new Rectangle(label1.Left - barCodeImg.Left, label1.Top - barCodeImg.Top, label1.Width, label1.Height);
                if (label1.BackColor != Color.Transparent)
                {
                    using (Brush bgBrush = new SolidBrush(label1.BackColor))
                    {
                        g.FillRectangle(bgBrush, rect);
                    }
                }
                g.DrawString(label1.Text, label1.Font, brush, rect, StringFormat.GenericDefault);
                g.Save();
            }
            img.Save("d:\\123.png", System.Drawing.Imaging.ImageFormat.Png);
        }

       


        // (3) 图像串口通信模块

        private void tbxRecvData_TextChanged(object sender, EventArgs e)
        {
            tbxRecvData.SelectionStart = tbxRecvData.Text.Length;
            tbxRecvData.ScrollToCaret();
        }


        private void btnCheckCOM_Click(object sender, EventArgs e)
        {
            bool comExistence = false;  //有可用串口标志位
            cbbCOMPort.Items.Clear();
            for (int i = 0; i < 30; i++)
            {
                try
                {
                    SerialPort sp = new SerialPort("COM" + (i + 1).ToString());
                    sp.Open();
                    sp.Close();
                    cbbCOMPort.Items.Add("COM" + (i + 1).ToString());
                    comExistence = true;
                }
                catch (Exception)
                {
                    continue;
                }
            }
            if (comExistence)
            {
                cbbCOMPort.SelectedIndex = 0;//使ListBox显示第一个添加的索引
            }
        }
        private bool CheckPortSetting() //检查串口是否设置
        {
            if (cbbCOMPort.Text.Trim() == "") return false;
            if (cbbBaudRate.Text.Trim() == "") return false;
            if (cbbDataBits.Text.Trim() == "") return false;
            if (cbbParity.Text.Trim() == "") return false;
            if (cbbStopBits.Text.Trim() == "") return false;
            return true;
        }

        private void SetPortProperty()  //设置串口的属性
        {
            sp = new SerialPort();
            sp.PortName = cbbCOMPort.Text.Trim();//设置串口名

            sp.BaudRate = Convert.ToInt32(cbbBaudRate.Text.Trim());//设置串口波特率

            float f = Convert.ToSingle(cbbStopBits.Text.Trim());   //设置停止位
            if (0 == f)
            {
                sp.StopBits = StopBits.None;
            }
            else if (1.5 == f)
            {
                sp.StopBits = StopBits.OnePointFive;
            }
            else if (1 == f)
            {
                sp.StopBits = StopBits.One;
            }
            else if (2 == f)
            {
                sp.StopBits = StopBits.Two;
            }
            else
            {
                sp.StopBits = StopBits.One;
            }
            sp.DataBits = Convert.ToInt16(cbbDataBits.Text.Trim());//设置数据位
            string s = cbbParity.Text.Trim();//设置奇偶校验位

            if (0 == s.CompareTo("无"))
            {
                sp.Parity = Parity.None;
            }
            else if (0 == s.CompareTo("奇校验"))
            {
                sp.Parity = Parity.Odd;
            }
            else if (0 == s.CompareTo("偶校验"))
            {
                sp.Parity = Parity.Even;
            }
            else
            {
                sp.Parity = Parity.None;
            }

            sp.ReadTimeout = -1;//设置超时读取时间
            sp.RtsEnable = true;
            //定义DataReceived事件，当串口收到数据后触发事件
            sp.DataReceived += new SerialDataReceivedEventHandler(sp_DataReceived);
            if (rbtnHex.Checked)
            {
                isHex = true;
            }
            else
            {
                isHex = false;
            }


        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            if (false == isOpen)
            {
                if (!CheckPortSetting()) //检查串口设置
                {
                    MessageBox.Show("串口未设置！", "错误提示");
                    return;
                }
                if (!isSetProperty)  //串口未设置则设置串口
                {
                    SetPortProperty();
                    isSetProperty = true;
                }
                try //打开串口
                {
                    sp.Open();
                    isOpen = true;
                    btnOpen.Text = "关闭串口";
                    //串口打开后，相关的串口设置按钮便不可再用  

                    cbbCOMPort.Enabled = false;
                    cbbBaudRate.Enabled = false;
                    cbbDataBits.Enabled = false;
                    cbbParity.Enabled = false;
                    cbbStopBits.Enabled = false;
                    rbtnHex.Enabled = false;
                }
                catch (Exception)
                {
                    //打开串口失败后，相应标志位取消
                    isSetProperty = false;
                    isOpen = false;
                    MessageBox.Show("串口无效或已被占用!", "错误提示");
                }
            }
            else
            {
                try //关闭串口
                {
                    sp.Close();
                    isOpen = false;
                    isSetProperty = false;
                    btnOpen.Text = "打开串口";
                    //关闭串口后，串口设置选项便可以继续使用
                    cbbCOMPort.Enabled = true;
                    cbbBaudRate.Enabled = true;
                    cbbDataBits.Enabled = true;
                    cbbParity.Enabled = true;
                    cbbStopBits.Enabled = true;
                    rbtnHex.Enabled = true;
                }
                catch (Exception)
                {
                    MessageBox.Show("关闭串口时发生错误！", "错误提示");
                }
            }
        }




        byte[] bytes;
        List<byte> list = new List<byte>();

        private void sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            stopwatch2.Start();
            //System.Threading.Thread.Sleep(9000);//延时100ms等待接收完数据
            //this.Invoke就是跨线程访问ui的方法，也是本文的范例
            this.Invoke(new EventHandler(delegate
            {
                //Byte[] 
                //if (sp.BytesToRead > 0)
                {

                    ReceivedData = new byte[sp.BytesToRead]; //创建接收字节数组
                    sp.Read(ReceivedData, 0, ReceivedData.Length);  //读取所接收到的数据


                    string RecvDataText = null;
                    if (false == isHex)
                    {
                        for (int i = 0; i < ReceivedData.Length; i++)
                        {
                            RecvDataText += ReceivedData[i];
                        }
                        //byte类型转成string类型
                        RecvDataText = System.Text.Encoding.Default.GetString(ReceivedData);
                        tbxRecvData.Text += RecvDataText;//更新接收框数据
                        tbxRecvLength.Text = tbxRecvData.TextLength.ToString();//更新接收框
                    }
                    else
                    {
                        list.AddRange(ReceivedData);  //修改2
                        for (int i = 0; i < ReceivedData.Length; i++)
                        {
                            RecvDataText += (ReceivedData[i].ToString("X2") + " ");
                        }

                        tbxRecvData.Text += RecvDataText;//更新接收框数据
                        tbxRecvLength.Text = (tbxRecvData.TextLength / 3).ToString();//更新接收框数据长度

                    }

                }
                stopwatch2.Stop();
                timebox2.Text = stopwatch2.ElapsedMilliseconds.ToString("####.##") + "毫秒";
            }
            ));
        }


        private void btnClearRev_Click(object sender, EventArgs e)
        {

            tbxRecvData.Text = "";
            //tbxSendData.Text = "";
            tbxRecvLength.Text = "0";//更新接收框数据长度
            //ptbOv7670.Image = OvImage;
            barCodeImg.Image = null;
            list = new List<byte>();
            timebox3.Text = null;
            timebox2.Text = null;
            stopwatch.Reset();
            stopwatch2.Reset();
        }

       

        


        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void tbxRecvLength_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2txtShowData_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtShowData_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnSend_Click(object sender, EventArgs e)
        {

        }

        private void rbtnHex_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click_1(object sender, EventArgs e)
        {

        }
        private void label6_Click(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void label14_Click(object sender, EventArgs e)
        {

        }

        private void ptbOv7670_Click(object sender, EventArgs e)
        {

        }

        private void imagesave_Click(object sender, EventArgs e)
        {
           
        }

        private void label16_Click(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void chkAutoLine_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void bindingNavigatorAddNewItem_Click(object sender, EventArgs e)
        {

        }

        private void bindingNavigator1_RefreshItems(object sender, EventArgs e)
        {

        }





        private void button11_Click(object sender, EventArgs e)
        {
            stopwatch.Start();

            this.Invoke((EventHandler)delegate
            {
                {

                    //for (int i = 0; i < int.Parse(tbxRecvLength.Text); i++)
                    bytes = list.ToArray(); //修改3
                    byte DataH;
                    for (int i = 0; i < bytes.Length; i++)
                    {

                        Int32 column = i % 120; //计算列数

                        Int32 row = (i / 120);       //计算行数 


                        if (bytes[i] > 22) { DataH = 255; }
                        else { DataH = 0; }
                    
                        //Int32 DataH = bytes[i];



                        Color newColorH = Color.FromArgb(DataH, DataH, DataH);  // 根据数据得到像素信息
                        OvImage.SetPixel(column, row, newColorH);  // 根据行列数和像素值还原灰度图


                    }
                    barCodeImg.Image = OvImage;
                    stopwatch.Stop();
                    timebox3.Text = stopwatch.ElapsedMilliseconds.ToString("####.##") + "毫秒";

                    sp.DiscardInBuffer();   //丢弃接收缓冲区数据     
                }
            });
        }

        private void groupBox5_Enter(object sender, EventArgs e)
        {

        }

        private void pictureBox6_Click(object sender, EventArgs e)
        {

        }

        private void label21_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox15_Click(object sender, EventArgs e)
        {

        }

        private void label12_Click(object sender, EventArgs e)
        {

        }

        private void label19_Click(object sender, EventArgs e)
        {

        }

        private void groupBox4_Enter(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click_1(object sender, EventArgs e)
        {

        }

        private void pictureBox14_Click(object sender, EventArgs e)
        {

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label20_Click(object sender, EventArgs e)
        {

        }

        private void label14_Click_1(object sender, EventArgs e)
        {

        }

        private void pictureBox11_Click(object sender, EventArgs e)
        {

        }

        private void tabPage3_Click(object sender, EventArgs e)
        {

        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {

        }

        private void label15_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click_1(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged_1(object sender, EventArgs e)
        {

        }

        private void label14_Click_2(object sender, EventArgs e)
        {

        }

        private void ContentTxt_TextChanged(object sender, EventArgs e)
        {

        }
    }
}