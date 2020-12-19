using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace IsochronDrafter
{
    public class DeckBuilder : Panel
    {
        private static readonly int NUM_INITIAL_COLUMNS = 8;
        private static readonly Color INDICATOR_COLOR = Color.Gold;
        public static readonly Color DRAGGED_STROKE_COLOR = Color.Gold;
        public static readonly int DRAGGED_STROKE_THICKNESS = 5;

        public DraftWindow draftWindow;
        public CardWindow cardWindow;
        private readonly List<List<DeckBuilderCard>[]> columns;
        private CardPosition dragged = CardPosition.None;
        private CardPosition hovered = CardPosition.None;
        private readonly PictureBox indicator;

        public DeckBuilder()
        {
            AutoScroll = true;

            columns = new List<List<DeckBuilderCard>[]>();
            for (int i = 0; i < NUM_INITIAL_COLUMNS; i++)
            {
                columns.Add(new List<DeckBuilderCard>[2]);
                columns[i][0] = new List<DeckBuilderCard>();
                columns[i][1] = new List<DeckBuilderCard>();
            }

            // Make indicator.
            indicator = new PictureBox { BackColor = INDICATOR_COLOR };
            Controls.Add(indicator);
            indicator.Hide();
        }

        public void AddCard(string cardName)
        {
            var card = new DeckBuilderCard
            {
                SizeMode = PictureBoxSizeMode.Zoom,
                Image = DraftWindow.GetImage(cardName),
                cardName = cardName,
                cmc = DraftWindow.GetCmc(cardName)
            };

            var colNum = Util.Clamp(0, card.cmc, ColumnCount - 2);
            columns[colNum][0].Add(card);
            Controls.Add(card);
            LayoutControls();
        }
        public void Clear()
        {
            Controls.Clear();
            for (int i = 0; i < columns.Count; i++)
            {
                columns[i] = new List<DeckBuilderCard>[2];
                columns[i][0] = new List<DeckBuilderCard>();
                columns[i][1] = new List<DeckBuilderCard>();
            }
        }
        public void SetNumColumns(int numColumns)
        {
            while (columns.Count > numColumns)
            {
                // Remove second-to-last column.
                List<DeckBuilderCard>[] column = columns[columns.Count - 2];
                columns.RemoveAt(columns.Count - 2);
                columns[columns.Count - 2][0].AddRange(column[0]);
                columns[columns.Count - 2][1].AddRange(column[1]);
            }
            while (columns.Count < numColumns)
            {
                // Add new second-to-last column.
                List<DeckBuilderCard>[] column = new List<DeckBuilderCard>[2];
                column[0] = new List<DeckBuilderCard>();
                column[1] = new List<DeckBuilderCard>();
                columns.Insert(columns.Count - 1, column);
            }
            LayoutControls();
        }

        protected override void OnResize(EventArgs eventargs)
        {
            LayoutControls();
            base.OnResize(eventargs);
        }
        protected override void OnInvalidated(InvalidateEventArgs e)
        {
            LayoutControls();
            base.OnInvalidated(e);
        }
        private void LayoutControls()
        {
            AutoScrollMargin = new Size(0, VerticalScroll.Visible ? SystemInformation.HorizontalScrollBarHeight : 0);

            DeckBuilderLayout layout = new DeckBuilderLayout(this);

            for (int column = 0; column < columns.Count; column++)
                for (int row = 0; row < 2; row++)
                    for (int cardNum = 0; cardNum < columns[column][row].Count; cardNum++)
                    {
                        // Set location and size.
                        DeckBuilderCard card = columns[column][row][cardNum];
                        layout.GetCardLeftAndTop(new CardPosition(column, row, cardNum), out var left, out var top);
                        card.Left = (int)Math.Round(left);
                        card.Top = (int)Math.Round(top);
                        card.Width = (int)Math.Round(layout.cardWidth);
                        card.Height = (int)Math.Round(layout.cardHeight);

                        // Set child index.
                        Controls.SetChildIndex(card, columns[column][row].Count - cardNum);
                    }

            indicator.BringToFront();
            SetCardCounts();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                var draggedPosition = GetPosFromClickCoor(e.X, e.Y, false);
                var card = TryGetCardFromPos(draggedPosition);
                if (card == null)
                    return;
                dragged = draggedPosition;
                card.selected = true;
                card.Invalidate();
            }
            else if (e.Button == MouseButtons.Middle || e.Button == MouseButtons.Right)
            {
                var card = TryGetCardFromPos(GetPosFromClickCoor(e.X, e.Y, false));
                if (card == null)
                    return;

                // Reposition card form and draw.
                cardWindow.SetImage(DraftWindow.GetImage(card.cardName));
                float x = card.Left + card.Width / 2f;
                float y = card.Top + card.Height / 2f;
                Point point = PointToScreen(new Point((int)Math.Round(x), (int)Math.Round(y)));
                cardWindow.SetLocation(point);
                cardWindow.Show();
                Focus();
            }
        }

        private DeckBuilderCard TryGetCardFromPos(CardPosition pos)
        {
            return pos == CardPosition.None ? null : columns[pos.Col][pos.Row].ElementAtOrDefault(pos.Num);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (dragged == CardPosition.None)
                return;
            var columnRowNum = GetPosFromClickCoor(e.X, e.Y, true);
            // Check if the hovered area has changed.
            if (columnRowNum == hovered)
                return;
            // Toggle visibility of indicator or change position.
            hovered = columnRowNum;
            if (hovered == CardPosition.None)
                indicator.Hide();
            else
            {
                // If the hovered position is immediately before or after the dragged card, don't draw the indicator.
                if (hovered.Col == dragged.Col && hovered.Row == dragged.Row && (hovered.Num == dragged.Num || hovered.Num == dragged.Num + 1))
                {
                    indicator.Hide();
                }
                // Otherwise, draw the indicator.
                else
                {
                    DeckBuilderLayout layout = new DeckBuilderLayout(this);
                    layout.GetCardLeftAndTop(hovered, out var left, out var top);
                    if (hovered.Num != 0 && hovered.Num == columns[hovered.Col][hovered.Row].Count)
                        // draw at bottom of last card
                        top += layout.cardHeight - layout.headerSize;
                    indicator.Left = (int)Math.Round(left - 2);
                    indicator.Top = (int)Math.Round(top - 1);
                    indicator.Width = (int)Math.Round(layout.cardWidth + 4);
                    indicator.Height = 2;
                    indicator.Show();
                }
            }
            Invalidate();
        }
        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button == MouseButtons.Left)
                MoveDraggedCardToHover();
            else if (e.Button == MouseButtons.Middle || e.Button == MouseButtons.Right)
                cardWindow.Hide();
        }

        private void MoveDraggedCardToHover()
        {
            if (dragged == CardPosition.None)
                return;

            var draggedCard = TryGetCardFromPos(dragged);
            if (hovered != CardPosition.None && hovered != dragged)
            {
                if (dragged.Col != hovered.Col || dragged.Row != hovered.Row)
                {
                    columns[dragged.Col][dragged.Row].RemoveAt(dragged.Num);
                    columns[hovered.Col][hovered.Row].Insert(hovered.Num, draggedCard);
                }
                else
                {
                    var cellList = columns[dragged.Col][dragged.Row];
                    cellList.RemoveAt(dragged.Num);
                    var insertNum = dragged.Num >= hovered.Num ? hovered.Num : hovered.Num - 1;
                    cellList.Insert(insertNum, draggedCard);
                }
            }

            draggedCard.selected = false;
            draggedCard.Invalidate();
            dragged = CardPosition.None;
            hovered = CardPosition.None;
            indicator.Hide();
            Invalidate();
        }

        private CardPosition GetPosFromClickCoor(int x, int y, bool isDestination)
        {
            var layout = new DeckBuilderLayout(this);
            if (!layout.TryGetColumn(x, out var column))
                return CardPosition.None;
            y += VerticalScroll.Value;
            int row = y < layout.secondRowY || column == columns.Count - 1 ? 0 : 1;
            int cardNum;
            if (columns[column][row].Count == 0) // Dragged card should get put as the first element in the now-empty column.
                cardNum = 0;
            else
            {
                if (column == columns.Count - 1)
                    y -= (int)Math.Round(layout.spacing * (DeckBuilderLayout.SIDEBOARD_SPACING_MULTIPLIER - 1));
                if (row == 1)
                    y -= (int)Math.Round(layout.secondRowY);
                y -= (int)Math.Round(layout.spacing);
                if (y < 0)
                    return CardPosition.None;
                int count = columns[column][row].Count;
                cardNum = (int)Math.Floor(y / layout.headerSize);
                if (cardNum >= count)
                    cardNum = isDestination ? count : count - 1;
            }
            return new CardPosition(column, row, cardNum);
        }

        public int ColumnCount => columns.Count;
        private IEnumerable<List<DeckBuilderCard>[]> MaindeckColumns => columns.Take(ColumnCount - 1);

        public int GetMaxFirstRowLength()
        {
            return MaindeckColumns.Max(c => c[0].Count);
        }

        private void SetCardCounts()
        {
            var maindeck = MaindeckColumns
                .SelectMany(row => row)
                .Sum(list => list.Count);

            var sideboard = columns
                .Last()
                .First()
                .Count;

            if (draftWindow != null && maindeck + sideboard > 0)
                draftWindow.SetCardCounts(maindeck, sideboard);
        }

        public string GetCockatriceDeck()
        {
            Dictionary<string, int> quantities = new Dictionary<string, int>();
            Dictionary<string, int> sideboardQuantities = new Dictionary<string, int>();
            for (int column = 0; column < columns.Count; column++)
                for (int row = 0; row < 2; row++)
                    for (int cardNum = 0; cardNum < columns[column][row].Count; cardNum++)
                    {
                        Dictionary<string, int> dictionary = column == columns.Count - 1 ? sideboardQuantities : quantities;
                        string cardName = columns[column][row][cardNum].cardName;
                        if (dictionary.ContainsKey(cardName))
                            dictionary[cardName]++;
                        else
                            dictionary.Add(cardName, 1);
                    }
            string output = "";
            foreach (KeyValuePair<string, int> kvp in quantities)
                output += "\r\n" + kvp.Value + " " + kvp.Key;
            foreach (KeyValuePair<string, int> kvp in sideboardQuantities)
                output += "\r\nSB: " + kvp.Value + " " + kvp.Key;
            return output.Trim();
        }
    }

    internal class DeckBuilderCard : PictureBox
    {
        public string cardName;
        public int cmc;
        public bool selected = false;

        protected override void OnPaint(PaintEventArgs paintEventArgs)
        {
            paintEventArgs.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            base.OnPaint(paintEventArgs);
            if (selected)
            {
                ControlPaint.DrawBorder(paintEventArgs.Graphics, ClientRectangle,
                                      DeckBuilder.DRAGGED_STROKE_COLOR, DeckBuilder.DRAGGED_STROKE_THICKNESS, ButtonBorderStyle.Outset,
                                      DeckBuilder.DRAGGED_STROKE_COLOR, DeckBuilder.DRAGGED_STROKE_THICKNESS, ButtonBorderStyle.Outset,
                                      DeckBuilder.DRAGGED_STROKE_COLOR, DeckBuilder.DRAGGED_STROKE_THICKNESS, ButtonBorderStyle.Outset,
                                      DeckBuilder.DRAGGED_STROKE_COLOR, DeckBuilder.DRAGGED_STROKE_THICKNESS, ButtonBorderStyle.Outset);
            }
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_NCHITTEST = 0x0084;
            const int HTTRANSPARENT = (-1);

            if (m.Msg == WM_NCHITTEST)
            {
                m.Result = (IntPtr)HTTRANSPARENT;
            }
            else
            {
                base.WndProc(ref m);
            }
        }
    }

    internal class DeckBuilderLayout
    {
        public static readonly float CARD_HEADER_PERCENTAGE = .1f;
        private static readonly float SPACING_PERCENTAGE = .05f;
        public static readonly int SIDEBOARD_SPACING_MULTIPLIER = 3;
        private static readonly int INTER_ROW_SPACING_MULTIPLIER = 3;
        private static readonly int CARD_WIDTH = 375;
        private static readonly int CARD_HEIGHT = 523;

        public readonly float scale, spacing, headerSize, secondRowY, cardWidth, cardHeight;
        private readonly int columnCount, vScrollValue;

        public DeckBuilderLayout(DeckBuilder deckBuilder)
        {
            columnCount = deckBuilder.ColumnCount;
            vScrollValue = deckBuilder.VerticalScroll.Value;
            float usableWidth = deckBuilder.ClientSize.Width;
            scale = usableWidth * (1 - SPACING_PERCENTAGE) / (columnCount * CARD_WIDTH);
            spacing = (usableWidth * SPACING_PERCENTAGE) / (columnCount + 1 + (SIDEBOARD_SPACING_MULTIPLIER - 1) * 2);
            headerSize = CARD_HEIGHT * scale * CARD_HEADER_PERCENTAGE;
            int maxFirstRowLength = deckBuilder.GetMaxFirstRowLength();
            secondRowY = (spacing * INTER_ROW_SPACING_MULTIPLIER - 1) + (headerSize * (maxFirstRowLength - 1)) +
                         (CARD_HEIGHT * scale);

            cardWidth = CARD_WIDTH * scale;
            cardHeight = CARD_HEIGHT * scale;
        }

        public void GetCardLeftAndTop(CardPosition position, out float left, out float top)
        {
            left = spacing * (position.Col + 1) + (cardWidth * position.Col);
            top = spacing + (headerSize * position.Num);
            if (position.Col == columnCount - 1)
            {
                left += spacing * (SIDEBOARD_SPACING_MULTIPLIER - 1);
                top += spacing * (SIDEBOARD_SPACING_MULTIPLIER - 1);
            }

            if (position.Row == 1)
                top += secondRowY;
            top -= vScrollValue;
        }

        public bool TryGetColumn(int x, out int col)
        {
            if (Math.Floor(x / (cardWidth + spacing)) > columnCount - 1)
                x -= (int)Math.Round(spacing * (SIDEBOARD_SPACING_MULTIPLIER - 1));
            if (x % (cardWidth + spacing) < spacing)
            {
                col = -1;
                return false;
            }

            col = (int)Math.Floor(x / (cardWidth + spacing));
            return col < columnCount;
        }
    }

    public readonly struct CardPosition
    {
        public static readonly CardPosition None = new CardPosition(-1, -1, -1);
        public readonly int Col, Row, Num;

        public CardPosition(int col, int row, int num)
        {
            Col = col;
            Row = row;
            Num = num;
        }
        private bool Equals(CardPosition other)
        {
            return Col == other.Col && Row == other.Row && Num == other.Num;
        }

        public override bool Equals(object obj)
        {
            return obj is CardPosition other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return Col + 8 * Row + 16 * Num;
            }
        }

        public static bool operator ==(CardPosition left, CardPosition right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CardPosition left, CardPosition right)
        {
            return !left.Equals(right);
        }
    }
}
