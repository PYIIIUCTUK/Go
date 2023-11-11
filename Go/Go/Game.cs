using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Go
{
    public partial class Game : Form
    {
        Menu menu;

        int S, W, H;
        int offsetX;
        int offsetY;

        int mouseX;
        int mouseY;

        Player curPlayer;

        List<Player> players = new List<Player>();
        List<List<Cell>> map = new List<List<Cell>>();

        List<Cell> checkedCells = new List<Cell>();
        List<Cell> deadCells = new List<Cell>();
        Point preCell = new Point(-1, -1);

        public Game(Menu Menu, int w, int h)
        {
            InitializeComponent();

            menu = Menu;
            W = w;
            H = h;
        }

        private void Game_Shown(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Maximized;
            DoubleBuffered = true;
            FormBorderStyle = FormBorderStyle.Sizable;

            S = Math.Min(ClientSize.Height / H, ClientSize.Width / W);
            offsetX = (ClientSize.Width - S * W) / 2;
            offsetY = (ClientSize.Height % S) / 2;

            for (int i = 0; i < H; i++)
            {
                List<Cell> line = new List<Cell>();
                for (int j = 0; j < W; j++)
                {
                    line.Add(new Cell(0, j, i));
                }
                map.Add(line);
            }

            players.Add(new Player(1, Brushes.DarkOrange, Pens.White));
            players.Add(new Player(2, Brushes.Black, Pens.Black));

            curPlayer = players[0];
        }
        private void Game_FormClosed(object sender, FormClosedEventArgs e)
        {
            menu.Show();
        }

        private void Game_Paint(object sender, PaintEventArgs e)
        {
            Pen pen = new Pen(Brushes.Black, 3);
            for (int i = 0; i < map.Count; i++)
            {
                for (int j = 0; j < map[i].Count; j++)
                {
                    e.Graphics.DrawLine(pen, j * S + S / 2 + offsetX, i * S + offsetY,
                        j * S + S / 2 + offsetX, i * S + S + offsetY);
                    e.Graphics.DrawLine(pen, j * S + offsetX, i * S + S / 2 + offsetY,
                        j * S + S + offsetX, i * S + S / 2 + offsetY);

                    if (map[i][j].Value != 0)
                    {
                        e.Graphics.FillEllipse(players[map[i][j].Value - 1].Brush, j * S + offsetX,
                                                                i * S + offsetY, S, S);
                    }
                }
            }
            e.Graphics.DrawRectangle(pen, offsetX, offsetY, S * W, S * H);

            if (curPlayer.Ind == 1)
            {
                e.Graphics.DrawString($"Ход Белых", new Font("Times New Roman", 36, FontStyle.Bold),
                      Brushes.Black, new PointF(W * S + S / 2 + offsetX, S / 2 + S * (H - 1) / 2 + offsetY));
            }
            else
            {
                e.Graphics.DrawString($"Ход Черных", new Font("Times New Roman", 36, FontStyle.Bold),
                   Brushes.Black, new PointF(W * S + S / 2 + offsetX, S / 2 + S * (H - 1) / 2 + offsetY));
            }

            e.Graphics.DrawString($"Счет Белых: {players[0].Score}", new Font("Times New Roman", 22, FontStyle.Bold),
                      Brushes.Black, new PointF(W * S + S / 2 + offsetX, S / 2 + S * (H - 1) / 2 + 2 * S + offsetY));
            e.Graphics.DrawString($"Счет Черных: {players[1].Score}", new Font("Times New Roman", 22, FontStyle.Bold),
                     Brushes.Black, new PointF(W * S + S / 2 + offsetX, S / 2 + S * (H - 1) / 2 - 2 * S + offsetY));

            if (mouseX >= 0 && mouseY >= 0)
            {
                e.Graphics.FillEllipse(curPlayer.Brush, mouseX * S + offsetX + S / 6,
                                       mouseY * S + offsetY + S / 6, S / (float)1.5, S / (float)1.5);
            }
        }

        private void Game_MouseClick(object sender, MouseEventArgs e)
        {
            if (mouseX < 0 || mouseY < 0) { return; }
            if (mouseX == preCell.X && mouseY == preCell.Y) { return; }
            if (map[mouseY][mouseX].Value != 0) { return; }

            if (CheckAllDead(mouseX, mouseY))
            {
                curPlayer.Score += 1;
                curPlayer.Score += deadCells.Count;
                preCell.X = deadCells[0].X;
                preCell.Y = deadCells[0].Y;
                DeadCells();

                map[mouseY][mouseX].Value = curPlayer.Ind;

                Invalidate();
                ChangeTurn();
            }
            else if (CheckAllNearest(mouseX, mouseY))
            {
                curPlayer.Score += 1;

                map[mouseY][mouseX].Value = curPlayer.Ind;

                preCell.X = -1;
                preCell.Y = -1;

                Invalidate();
                ChangeTurn();
            }
            UnchectedCells();
        }
        private void Game_MouseMove(object sender, MouseEventArgs e)
        {
            int x = (e.X - offsetX);
            int y = (e.Y - offsetY);
            if (x < 0 || x >= W * S || y < 0 || y >= H * S)
            { mouseX = -1; mouseY = -1; Invalidate(); return; }

            x /= S;
            y /= S;

            if (x != mouseX || y != mouseY)
            {
                mouseX = x;
                mouseY = y;
                Invalidate();
            }
        }

        private bool CheckAllNearest(int X, int Y)
        {
            int x = X;
            int y = Y;

            map[y][x].Checked = true;
            checkedCells.Add(map[y][x]);

            x = X;
            y = Y + 1;
            if (CheckNearest(x, y)) { return true; }

            x = X;
            y = Y - 1;
            if (CheckNearest(x, y)) { return true; }

            x = X - 1;
            y = Y;
            if (CheckNearest(x, y)) { return true; }

            x = X + 1;
            y = Y;
            if (CheckNearest(x, y)) { return true; }

            return false;
        }
        private bool CheckNearest(int x, int y)
        {
            if (x >= 0 && x < W && y >= 0 && y < H)
            {
                if (!map[y][x].Checked && map[y][x].Value == 0)
                {
                    return true;
                }
                else if (!map[y][x].Checked && map[y][x].Value == curPlayer.Ind)
                {
                    if (CheckAllNearest(x, y)) { return true; }
                }
            }

            return false;
        }

        private bool CheckAllDead(int X, int Y)
        {
            ChangePlayer();
            map[mouseY][mouseX].Checked = true;
            checkedCells.Add(map[mouseY][mouseX]);

            bool res = false;
            int x = X;
            int y = Y + 1;
            int num = checkedCells.Count;

            if (CheckDead(x, y, num)) { res = true; }
            num = checkedCells.Count;

            x = X;
            y = Y - 1;
            if (CheckDead(x, y, num)) { res = true; }
            num = checkedCells.Count;

            x = X - 1;
            y = Y;
            if (CheckDead(x, y, num)) { res = true; }
            num = checkedCells.Count;

            x = X + 1;
            y = Y;
            if (CheckDead(x, y, num)) { res = true; }

            curPlayer.Score -= deadCells.Count;
            ChangePlayer();
            UnchectedCells();
            return res;
        }
        private bool CheckDead(int x, int y, int num)
        {
            if (x >= 0 && x < W && y >= 0 && y < H)
            {
                if (!map[y][x].Checked && map[y][x].Value == curPlayer.Ind)
                {
                    if (!CheckAllNearest(x, y))
                    {
                        AddToDead(num);
                        return true;
                    }
                }
            }
            return false;
        }
        private void AddToDead(int ind)
        {
            for (int i = ind; i < checkedCells.Count; i++)
            {
                deadCells.Add(checkedCells[i]);
            }
        }

        private void UnchectedCells()
        {
            foreach (Cell cell in checkedCells)
            {
                cell.Checked = false;
            }
            checkedCells.Clear();
        }
        private void DeadCells()
        {
            foreach (Cell cell in deadCells)
            {
                cell.Value = 0;
            }
            deadCells.Clear();
        }

        private bool CheckWin()
        {
            if (!CheckCanMove())
            {
                if (players[0].Score > players[1].Score)
                {
                    MessageBox.Show("White Win");
                    return true;
                }
                else if (players[0].Score < players[1].Score)
                {
                    MessageBox.Show("Black Win");
                    return true;
                }
                else
                {
                    MessageBox.Show("Draw");
                    return true;
                }
            }
            return false;
        }
        private bool CheckCanMove()
        {
            for (int i = 0; i < H; i++)
            {
                for (int j = 0; j < W; j++)
                {
                    if (map[i][j].Value == 0)
                    {
                        if (CheckAllDead(j, i) || CheckAllNearest(j, i))
                        {
                            UnchectedCells();
                            return true;
                        }
                        UnchectedCells();
                    }
                }
            }
            return false;
        }

        private void ChangePlayer()
        {
            for (int i = 0; i < players.Count; i++)
            {
                if (curPlayer.Ind != players[i].Ind)
                {
                    curPlayer = players[i];
                    break;
                }
            }
        }
        private void ChangeTurn()
        {
            ChangePlayer();
            if (CheckWin())
            {
                Close();
            }
        }
    }
}
