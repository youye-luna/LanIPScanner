using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace DhcpScanner
{
    public class IPGridPanel : Panel
    {
        private readonly Color[] _cellColors;
        private const int TotalCells = 255;
        private const int Cols = 16;
        private int Rows => (TotalCells + Cols - 1) / Cols;
        private const int GridPadding = 4;

        /// <summary>
        /// 格子被单击事件（用于定位）
        /// </summary>
        public event Action<int>? CellClicked;

        /// <summary>
        /// 格子被双击事件（用于弹出详情）
        /// </summary>
        public event Action<int>? CellDoubleClicked;

        private int _selectedIndex = -1;

        public IPGridPanel()
        {
            _cellColors = new Color[TotalCells];
            for (int i = 0; i < TotalCells; i++)
                _cellColors[i] = Color.FromArgb(240, 240, 240);

            AutoScroll = true;

            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);
        }

        public void SetIpColor(int ipIndex, Color color)
        {
            if (ipIndex >= 0 && ipIndex < TotalCells)
            {
                _cellColors[ipIndex] = color;
                Invalidate();
            }
        }

        public void ResetAllColors()
        {
            for (int i = 0; i < TotalCells; i++)
                _cellColors[i] = Color.FromArgb(240, 240, 240);
            Invalidate();
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            int index = HitTestCell(e.Location);
            if (index >= 0)
            {
                _selectedIndex = index;
                Invalidate();
                CellClicked?.Invoke(index);
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            int index = HitTestCell(e.Location);
            if (index >= 0)
            {
                _selectedIndex = index;
                Invalidate();
                CellDoubleClicked?.Invoke(index);
            }
        }

        /// <summary>
        /// 将点击坐标转换为格子索引，未命中返回 -1
        /// </summary>
        private int HitTestCell(Point point)
        {
            int cellSize = (Width - GridPadding * 2) / Cols;
            cellSize = Math.Max(cellSize, 12);
            int gridWidth = cellSize * Cols;
            int startX = (Width - gridWidth) / 2;
            int titleHeight = 22;
            int scrollY = AutoScrollPosition.Y;
            int startY = scrollY + titleHeight;

            int col = (point.X - startX) / cellSize;
            int row = (point.Y - startY) / cellSize;
            if (col < 0 || col >= Cols || row < 0 || row >= Rows) return -1;

            int index = row * Cols + col;
            return index >= 0 && index < TotalCells ? index : -1;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // 根据面板宽度动态计算格子大小
            int cellSize = (Width - GridPadding * 2) / Cols;
            cellSize = Math.Max(cellSize, 12);
            int cellSizeInner = cellSize - 1;

            int gridWidth = cellSize * Cols;
            int gridHeight = cellSize * Rows;
            int startX = (Width - gridWidth) / 2;
            int titleHeight = 22;
            int legendHeight = 22;
            int totalHeight = titleHeight + gridHeight + legendHeight + GridPadding;

            // 设置滚动区域（仅在内容超出时显示滚动条）
            AutoScrollMinSize = new System.Drawing.Size(0, totalHeight);

            int scrollY = AutoScrollPosition.Y;

            // 标题
            using var titleFont = new Font("Microsoft YaHei", 9F, FontStyle.Bold);
            string title = "IP 地址分布图";
            var titleSize = g.MeasureString(title, titleFont);
            g.DrawString(title, titleFont, Brushes.DimGray,
                (Width - titleSize.Width) / 2, scrollY + 2);

            int startY = scrollY + titleHeight;

            // 字体大小根据格子自动适配
            float fontSize = cellSize >= 22 ? 8F : cellSize >= 16 ? 7F : 6F;
            using var numFont = new Font("Microsoft YaHei", fontSize, FontStyle.Bold);

            for (int i = 0; i < TotalCells; i++)
            {
                int row = i / Cols;
                int col = i % Cols;
                int x = startX + col * cellSize;
                int y = startY + row * cellSize;

                var rect = new Rectangle(x, y, cellSizeInner, cellSizeInner);
                if (rect.Width > 0 && rect.Height > 0)
                {
                    using var brush = new SolidBrush(_cellColors[i]);
                    g.FillRectangle(brush, rect);
                }

                // 选中格子高亮边框
                if (i == _selectedIndex)
                {
                    using var pen = new Pen(Color.FromArgb(255, 87, 34), 2f);
                    g.DrawRectangle(pen, rect);
                }

                // 显示数字
                string text = (i + 1).ToString();
                var textSize = g.MeasureString(text, numFont);
                if (textSize.Width < cellSizeInner && textSize.Height < cellSizeInner)
                {
                    float tx = x + (cellSizeInner - textSize.Width) / 2;
                    float ty = y + (cellSizeInner - textSize.Height) / 2;
                    var bgColor = _cellColors[i];
                    float brightness = (bgColor.R * 0.299f + bgColor.G * 0.587f + bgColor.B * 0.114f) / 255f;
                    var textColor = brightness < 0.5f ? Brushes.White : Brushes.Black;
                    g.DrawString(text, numFont, textColor, tx, ty);
                }
            }

            // 图例
            int legendY = startY + gridHeight + 3;
            int legendX = startX;
            int dotSize = 9;

            var legendItems = new (Color color, string text)[]
            {
                (Color.FromArgb(240, 240, 240), "未扫描"),
                (Color.FromArgb(76, 175, 80), "无设备"),
                (Color.FromArgb(33, 150, 243), "在线"),
                (Color.FromArgb(255, 138, 128), "DHCP服务器"),
            };

            using var legendFont = new Font("Microsoft YaHei", 6.5F);

            foreach (var item in legendItems)
            {
                g.FillRectangle(new SolidBrush(item.color), legendX, legendY, dotSize, dotSize);
                g.DrawString(item.text, legendFont, Brushes.DimGray, legendX + dotSize + 2, legendY);
                legendX += (int)g.MeasureString(item.text, legendFont).Width + 16;
            }
        }
    }
}
