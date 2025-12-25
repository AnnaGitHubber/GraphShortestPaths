using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Drawing.Drawing2D; // Для стрілок

namespace GraphShortestPaths
{
    public partial class Form1 : Form
    {
        // =================================================================
        // 1. ГЛОБАЛЬНІ ЗМІННІ
        // =================================================================
        private Graph currentGraph;
        private const int INF = int.MaxValue / 4;

        // Масив для конвертації індексів у літерні позначення (0=a, 1=b, 2=c, ...)
        private readonly string[] VertexNames = { "a", "b", "c", "d", "e", "f" };

        private RichTextBox resultLog; // Для виведення результатів
        private PictureBox graphDisplay; // Для візуалізації графа (Рисунок 4)

        private Dictionary<int, Point> NodePositions; // Допоміжні змінні для малювання
        private const int NodeSize = 30; // Діаметр вершини

        // =================================================================
        // 2. КОНСТРУКТОР
        // =================================================================
        public Form1()
        {
            // Цей виклик є критичним для ініціалізації форми
            InitializeComponent();

            // Налаштування форми
            this.Text = "DFS/BFS Visualization - Shortest Paths";
            this.Size = new Size(950, 700);
            this.StartPosition = FormStartPosition.CenterScreen;

            // Створення елементів інтерфейсу
            CreateGraphDisplayPanel();
            CreateResultLogPanel();
            CreateControlButtons();
        }

        // =================================================================
        // 3. МЕТОДИ СТВОРЕННЯ UI
        // =================================================================
        private void CreateGraphDisplayPanel()
        {
            graphDisplay = new PictureBox();
            graphDisplay.BorderStyle = BorderStyle.FixedSingle;
            graphDisplay.Location = new Point(10, 10);
            graphDisplay.Size = new Size(530, 580);
            this.Controls.Add(graphDisplay);
        }

        private void CreateResultLogPanel()
        {
            // Панель для заголовка та логу
            Panel resultPanel = new Panel();
            resultPanel.BorderStyle = BorderStyle.FixedSingle;
            resultPanel.Location = new Point(550, 10);
            resultPanel.Size = new Size(370, 580);

            Label headerLabel = new Label();
            headerLabel.Text = "Лог / Результат:";
            headerLabel.Location = new Point(5, 5);
            headerLabel.AutoSize = true;

            resultLog = new RichTextBox();
            resultLog.Location = new Point(5, 30);
            resultLog.Size = new Size(360, 545);
            resultLog.ReadOnly = true;

            resultPanel.Controls.Add(headerLabel);
            resultPanel.Controls.Add(resultLog);
            this.Controls.Add(resultPanel);
        }

        private void CreateControlButtons()
        {
            int startX = 10;
            int startY = 610;
            int buttonHeight = 35;

            // --- Блок 1: Створення графа ---
            AddButton("Створити орієнтований", ref startX, startY, 160, buttonHeight, BtnCreateDirected_Click);
            AddButton("Створити неорієнтований", ref startX, startY, 160, buttonHeight, BtnCreateUndirected_Click);

            // --- Блок 2: Обхід ---
            AddButton("DFS", ref startX, startY, 60, buttonHeight, BtnDFS_Click);
            AddButton("BFS", ref startX, startY, 60, buttonHeight, BtnBFS_Click);

            // --- Блок 3: Алгоритми шляхів ---
            AddButton("Dijkstra from a", ref startX, startY, 120, buttonHeight, BtnDijkstra_Click);
            AddButton("Floyd-Warshall", ref startX, startY, 120, buttonHeight, BtnFloydWarshall_Click);
        }

        // Допоміжний метод для створення кнопок
        private void AddButton(string text, ref int x, int y, int width, int height, EventHandler clickHandler)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Location = new Point(x, y);
            btn.Size = new Size(width, height);
            btn.Click += clickHandler;
            this.Controls.Add(btn);
            x += width + 5; // Збільшуємо позицію X для наступної кнопки
        }

        // =================================================================
        // 4. ОБРОБНИКИ ПОДІЙ (ЛОГІКА)
        // =================================================================

        // --- А. Малювання графа ---
        private void DrawGraph(Graph g)
        {
            // Очищаємо PictureBox та готуємо GDI+
            graphDisplay.Image = new Bitmap(graphDisplay.Width, graphDisplay.Height);
            Graphics gdi = Graphics.FromImage(graphDisplay.Image);
            gdi.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // --- 1. ОНОВЛЕНІ ПОЗИЦІЇ ВЕРШИН (за макетом зображення 2) ---
            // (a=0, b=1, c=2, d=3, e=4, f=5)
            NodePositions = new Dictionary<int, Point>
    {
        {0, new Point(150, 100)}, // a
        {1, new Point(350, 100)}, // b
        {2, new Point(100, 300)}, // c
        {3, new Point(400, 300)}, // d
        {4, new Point(250, 450)}, // e
        {5, new Point(250, 250)}  // f (в центрі)
    };

            // --- 2. Малювання ребер та ваг ---
            System.Drawing.Drawing2D.AdjustableArrowCap arrow =
    new System.Drawing.Drawing2D.AdjustableArrowCap(5, 5, true);
            Pen edgePen = new Pen(Color.Gray, 2);
            edgePen.CustomEndCap = arrow;

            Font weightFont = new Font("Arial", 10, FontStyle.Bold);
            SolidBrush textBrush = new SolidBrush(Color.Red);

            for (int u = 0; u < g.VerticesCount; u++)
            {
                Point p1 = NodePositions[u];
                var adjList = g.GetAdjWithWeights(u);

                if (adjList != null)
                {
                    foreach (var edge in adjList)
                    {
                        Point p2 = NodePositions[edge.to];

                        gdi.DrawLine(edgePen, p1, p2);

                        // Обчислюємо позицію для ваги
                        Point midPoint = new Point((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);

                        // Зміщення для ваги
                        // Для горизонтальних ребер (a-b), зміщуємо трохи вгору/вниз
                        if (Math.Abs(p1.Y - p2.Y) < 50)
                            midPoint.Y -= 15;
                        else
                            midPoint.X -= 15; // Для вертикальних/діагональних

                        gdi.DrawString(edge.w.ToString(), weightFont, textBrush, midPoint);
                    }
                }
            }

            // --- 3. Малювання вершин та назв ---
            SolidBrush nodeBrush = new SolidBrush(Color.LightGray);
            Pen borderPen = new Pen(Color.Black, 2);
            Font nodeFont = new Font("Arial", 12);

            for (int i = 0; i < g.VerticesCount; i++)
            {
                Point pos = NodePositions[i];
                Rectangle rect = new Rectangle(pos.X - NodeSize / 2, pos.Y - NodeSize / 2, NodeSize, NodeSize);

                gdi.FillEllipse(nodeBrush, rect);
                gdi.DrawEllipse(borderPen, rect);

                string name = VertexNames[i];
                SizeF textSize = gdi.MeasureString(name, nodeFont);
                Point textPos = new Point(
                    pos.X - (int)(textSize.Width / 2),
                    pos.Y - (int)(textSize.Height / 2)
                );
                gdi.DrawString(name, nodeFont, Brushes.Black, textPos);
            }

            graphDisplay.Refresh();
        }

        // --- B. Ініціалізація графа (Варіант 2) ---
        private void BtnCreateDirected_Click(object sender, EventArgs e)
        {
            // 6 вершин (0=a, 1=b, 2=c, 3=d, 4=e, 5=f)
            currentGraph = new Graph(6);

            // Ребра згідно з варіантом 2:
            currentGraph.AddEdge(0, 1, 3); // a -> b (3)
            currentGraph.AddEdge(0, 2, 6); // a -> c (6)
            currentGraph.AddEdge(2, 5, 5); // c -> f (5)
            currentGraph.AddEdge(1, 3, 2); // b -> d (2)
            currentGraph.AddEdge(3, 5, 4); // d -> f (4)
            currentGraph.AddEdge(5, 4, 7); // f -> e (7)

            resultLog.Text = "Орієнтований граф (Варіант 2) успішно ініціалізовано.\n";

            // Виклик малювання
            DrawGraph(currentGraph);
        }

        // --- C. Дейкстра ---
        private void BtnDijkstra_Click(object sender, EventArgs e)
        {
            if (currentGraph == null) { resultLog.Text = "Спочатку створіть граф."; return; }

            int src = 0; // Початкова вершина 'a'
            (int[] dist, int[] prev) = Algorithms.Dijkstra(currentGraph, src);

            resultLog.Text = $"Dijkstra from {VertexNames[src]}:\n";

            for (int i = 0; i < currentGraph.VerticesCount; i++)
            {
                string startNode = VertexNames[src];
                string endNode = VertexNames[i];

                string pathString;
                string distValue = (dist[i] == INF) ? "INF" : dist[i].ToString();

                if (dist[i] == INF)
                {
                    pathString = "шлях відсутній";
                }
                else
                {
                    
                    var pathIndices = new List<int>();
                    int current = i;
                    while (current != -1)
                    {
                        pathIndices.Add(current);
                        if (current == src) break;
                        current = prev[current];
                    }
                    pathIndices.Reverse();
                    pathString = string.Join(" -> ", pathIndices.Select(idx => VertexNames[idx]));
                }

                resultLog.AppendText($"{startNode} -> {endNode}: dist = {distValue}; path = {pathString}\n");
            }
        }

        // --- D. Флойд-Уоршелл ---
        private void BtnFloydWarshall_Click(object sender, EventArgs e)
        {
            if (currentGraph == null) { resultLog.Text = "Спочатку створіть граф."; return; }

            (int[,] dist, int[,] next) = Algorithms.FloydWarshall(currentGraph);

            resultLog.Text = "Floyd-Warshall (all pairs)\n\n";
            resultLog.AppendText("Matrix dist[i, j]:\n");

            // 1. Вивід матриці відстаней
            string header = "   " + string.Join("   ", VertexNames);
            resultLog.AppendText(header + "\n");

            for (int i = 0; i < currentGraph.VerticesCount; i++)
            {
                string line = VertexNames[i] + "  ";
                for (int j = 0; j < currentGraph.VerticesCount; j++)
                {
                    string val = (dist[i, j] >= INF) ? "-" : dist[i, j].ToString();
                    line += val.PadLeft(3) + " ";
                }
                resultLog.AppendText(line + "\n");
            }

            // 2. Вивід шляхів від 'a' (індекс 0)
            resultLog.AppendText("\nPaths from a:\n");
            int src = 0;
            for (int dest = 0; dest < currentGraph.VerticesCount; dest++)
            {
                string startNode = VertexNames[src];
                string endNode = VertexNames[dest];

                if (dist[src, dest] >= INF)
                {
                    resultLog.AppendText($"{startNode} -> {endNode}: шлях відсутній\n");
                }
                else if (src == dest)
                {
                    resultLog.AppendText($"{startNode} -> {endNode}: {startNode}\n");
                }
                else
                {
                    // Відновлення шляху за допомогою матриці next
                    string path = startNode;
                    int u = src;
                    while (u != dest && next[u, dest] != -1)
                    {
                        u = next[u, dest];
                        path += " -> " + VertexNames[u];
                    }
                    resultLog.AppendText($"{startNode} -> {endNode}: {path}\n");
                }
            }
        }

        // --- E. Обробники для функціоналу, що не є частиною завдання ---
        private void BtnCreateUndirected_Click(object sender, EventArgs e) { resultLog.Text = "Створення неорієнтованого графа (логіка не реалізована в цьому прикладі)."; }
        private void BtnDFS_Click(object sender, EventArgs e) { resultLog.Text = "Обхід DFS (логіка не реалізована в цьому прикладі)."; }
        private void BtnBFS_Click(object sender, EventArgs e) { resultLog.Text = "Обхід BFS (логіка не реалізована в цьому прикладі)."; }
    }
}