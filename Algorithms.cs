using System;
using System.Collections.Generic;

public static class Algorithms
{
    private const int INF = int.MaxValue / 4; // Константа нескінченності

    public static (int[] dist, int[] prev) Dijkstra(Graph g, int src)
    {
        int n = g.VerticesCount;
        int[] dist = new int[n];
        int[] prev = new int[n];
        bool[] used = new bool[n];

        // Ініціалізація
        for (int i = 0; i < n; i++)
        {
            dist[i] = INF;
            prev[i] = -1;
        }
        dist[src] = 0;

        for (int it = 0; it < n; it++)
        {
            int v = -1;
            int best = INF;

            // 1. Пошук невідвіданої вершини з мінімальною відстанню
            for (int i = 0; i < n; i++)
            {
                if (!used[i] && dist[i] < best)
                {
                    best = dist[i];
                    v = i;
                }
            }

            if (v == -1) break; // Немає доступних вершин
            used[v] = true;

            // 2. Релаксація ребер
            foreach (var nb in g.GetAdjWithWeights(v))
            {
                int to = nb.to;
                int w = nb.w;
                if (dist[v] + w < dist[to])
                {
                    dist[to] = dist[v] + w;
                    prev[to] = v; // Зберігаємо попередника
                }
            }
        }
        return (dist, prev);
    }

    public static (int[,] dist, int[,] next) FloydWarshall(Graph g)
    {
        int n = g.VerticesCount;
        // Ініціалізація dist і next
        int[,] dist = g.ToWeightMatrix(INF);
        int[,] next = new int[n, n];

        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                next[i, j] = (i != j && dist[i, j] < INF) ? j : -1;
            }
        }

        // Три вкладені цикли (k, i, j)
        for (int k = 0; k < n; k++) // Проміжна вершина
        {
            for (int i = 0; i < n; i++) // Початкова вершина
            {
                if (dist[i, k] == INF) continue;

                for (int j = 0; j < n; j++) // Кінцева вершина
                {
                    if (dist[k, j] == INF) continue;

                    int newDist = dist[i, k] + dist[k, j];

                    if (newDist < dist[i, j])
                    {
                        dist[i, j] = newDist;
                        next[i, j] = next[i, k];
                    }
                }
            }
        }
        return (dist, next);
    }
}