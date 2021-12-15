﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;
using System.IO;
using System.Linq;
using UnityEngine;

public class Csv_Data : MonoBehaviour
{
    public string[] m_ColumnHeadings = { "시간", "마우스 X", "마우스 Y" };
    public bool m_IsColumnHeading, m_IsStopWrite;
    private List<string[]> m_WriteRowData = new List<string[]>();

    public string m_Path = Application.dataPath + "/Resources/";
    public string m_FilePrefix = "Mouse";
    private string m_FilePath;
    private bool m_IsWriting;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W) && !m_IsWriting)
        {
            if (m_IsColumnHeading)
            {
                m_WriteRowData.Add(m_ColumnHeadings);
            }

            StartCoroutine(CMakeRowBodys(m_ColumnHeadings.Length));
            m_IsWriting = true;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            m_IsStopWrite = true;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            string[,] readDatas = ReadCsv(m_FilePath);
            Debug.Log(readDatas[1, 1]);
        }
    }

    // Write Csv 
    IEnumerator CMakeRowBodys(int nRows)
    {
        int interval = 1;

        float seconds = 0;

        while (true)
        {
            string[] rowDataTemp = new string[nRows];

            seconds += interval;
            float mouseX = Input.mousePosition.x;
            float mouseY = Input.mousePosition.y;

            rowDataTemp[0] = seconds.ToString();
            rowDataTemp[1] = mouseX.ToString();
            rowDataTemp[2] = mouseY.ToString();

            CsvAddRow(rowDataTemp, m_WriteRowData);

            if (m_IsStopWrite)
            {
                m_FilePath
                    = m_Path + @"\" + m_FilePrefix +
                    DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv";

                WriteCsv(m_WriteRowData, m_FilePath);
                m_WriteRowData.Clear();
                break;
            }

            yield return new WaitForSeconds(interval);
        }
    }

    void CsvAddRow(string[] rows, List<string[]> rowData)
    {
        string[] rowDataTemp = new string[rows.Length];
        for (int i = 0; i < rows.Length; i++)
            rowDataTemp[i] = rows[i];
        rowData.Add(rowDataTemp);
    }

    public void WriteCsv(List<string[]> rowData, string filePath)
    {
        string[][] output = new string[rowData.Count][];

        for (int i = 0; i < output.Length; i++)
        {
            output[i] = rowData[i];
        }

        int length = output.GetLength(0);
        string delimiter = ",";

        StringBuilder stringBuilder = new StringBuilder();

        for (int index = 0; index < length; index++)
            stringBuilder.AppendLine(string.Join(delimiter, output[index]));

        Stream fileStream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write);
        StreamWriter outStream = new StreamWriter(fileStream, Encoding.UTF8);
        outStream.WriteLine(stringBuilder);
        outStream.Close();

        m_IsWriting = false;
    }

    // Read Csv 
    public string[,] ReadCsv(string filePath)
    {
        string value = "";
        StreamReader reader = new StreamReader(filePath, Encoding.UTF8);
        value = reader.ReadToEnd();
        reader.Close();

        string[] lines = value.Split("\n"[0]);

        int width = 0;
        for (int i = 0; i < lines.Length; i++)
        {
            string[] row = SplitCsvLine(lines[i]);
            width = Mathf.Max(width, row.Length);
        }

        string[,] outputGrid = new string[width + 1, lines.Length + 1];
        for (int y = 0; y < lines.Length; y++)
        {
            string[] row = SplitCsvLine(lines[y]);
            for (int x = 0; x < row.Length; x++)
            {
                outputGrid[x, y] = row[x];
                outputGrid[x, y] = outputGrid[x, y].Replace("\"\"", "\"");
            }
        }

        return outputGrid;
    }

    public string[] SplitCsvLine(string line)
    {
        return (from Match m in System.Text.RegularExpressions.Regex.Matches(line,
        @"(((?<x>(?=[,\r\n]+))|""(?<x>([^""]|"""")+)""|(?<x>[^,\r\n]+)),?)",
        RegexOptions.ExplicitCapture)
                select m.Groups[1].Value).ToArray();
    }
}