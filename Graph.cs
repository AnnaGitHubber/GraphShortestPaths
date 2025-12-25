using System.Collections.Generic;
using System;

// Структура для представлення сусіда з вагою
public class Adjacency
{
    public int to;  // Індекс кінцевої вершини
    public int w;   // Вага ребра
    public Adjacency(int to, int w)
    {
        this.to = to;
        this.w = w;
    }
}

// Клас для представлення графа
public class Graph
{
    private List<Adjacency>[] adj; // Список суміжності

    public int VerticesCount { get; private set; }

    // Конструктор
    public Graph(int n)
    {
        VerticesCount = n;
        adj = new List<Adjacency>[n];
        for (int i = 0; i < n; i++)
        {
            adj[i] = new List<Adjacency>();
        }
    }

    // Додавання орієнтованого ребра
    public void AddEdge(int u, int v, int weight)
    {
        adj[u].Add(new Adjacency(v, weight));
    }

    // Отримання списку суміжності для Дейкстри
    public List<Adjacency> GetAdjWithWeights(int v)
    {
        return adj[v];
    }

    // Створення матриці ваг для Флойда-Уоршелла
    public int[,] ToWeightMatrix(int inf)
    {
        int[,] matrix = new int[VerticesCount, VerticesCount];
        for (int i = 0; i < VerticesCount; i++)
        {
            for (int j = 0; j < VerticesCount; j++)
            {
                // Відстань від вершини до себе - 0. Інші - INF
                matrix[i, j] = (i == j) ? 0 : inf;
            }
        }
        // Заповнення існуючими вагами ребер
        for (int i = 0; i < VerticesCount; i++)
        {
            foreach (var edge in adj[i])
            {
                matrix[i, edge.to] = edge.w;
            }
        }
        return matrix;
    }
}