using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON; //A common JSON parser since Unity internal JsonUtility does not support nested arrays.
using System.IO;

public class JsonReader : MonoBehaviour
{
    enum E_Parser { SimpleJSON, JsonUtility };
    [SerializeField] E_Parser parser;
    //Used to let the other scripts know we just updated the JSON
    public delegate void LoadedNewJSON();
    public LoadedNewJSON loadedNewJSON;


    string filePath; //Path to the JSON File
    public string title { get; private set; } //The title inside the JSON File
    public string[,] matrix { get; private set; } //The matrix with all the json columns and rows inside the Data

    DateTime lastFileModify; //The time we last updated the JSON File. Used to check if we need to reload the file because it was modified 

    public static JsonReader self; //Singleton       
    private void Awake()
    {
        //Creeates the singleton in a safe way
        if(self == null)
        {
            self = this;
        }
        else
        {
            Debug.LogError("Duplicate singleton found. Deleting new one.");
            DestroyImmediate(this);
        }
    }

    void Start()
    {
        filePath = Path.Combine(Application.streamingAssetsPath, "JsonChallenge.json");
    }

    private void Update()
    {
        if(File.GetLastWriteTime(filePath) > lastFileModify) //Check if the file was modified and reads de JSON for the first time
        {
            switch (parser)
            {
                case E_Parser.SimpleJSON:
                    ReadWithSimpleJson();
                    break;
                case E_Parser.JsonUtility:
                    ReadWithJSONUtility();
                    break;
            }
        }
    }

    //This uses the SimpleJSON Library to parse the JSON since Unity JsonUtility does not support nested arrays. This is a common solution and way faster than rewriting a new complete parser.
    public void ReadWithSimpleJson()
    {
        lastFileModify = File.GetLastWriteTime(filePath); //Save the last modification time of the file to make sure we are reading the file when it is modified

        bool fileLoaded = false;
        StreamReader reader = null;
        string testJson = "";
        while (!fileLoaded) //We keep trying to read the JSON file. This is important because the file is modified before it finishes saving and the other program can still be accessing it and it will return a error. 
        { 
            try
            {
                reader = new StreamReader(filePath);
                testJson = reader.ReadToEnd();
                reader.Close();
                fileLoaded = true;
            }
            catch
            {
                fileLoaded = false;
            }
        }

        var json = JSON.Parse(testJson);
        title = json["Title"].Value;
        matrix = new string[json["ColumnHeaders"].Count, json["Data"].Count + 1]; //Creating our matrix array

        //Placing the headers acording to the ColumnHeaders
        for (int i = 0; i < json["ColumnHeaders"].Count; i++) 
        {
            matrix[i, 0] = json["ColumnHeaders"][i].Value;
        }

        //Placing the rows acording to the Data
        for (int k = 0; k < matrix.GetLength(1)-1; k++)//Rows
        {
            for (int i = 0; i < matrix.GetLength(0); i++)//Columns
            {
                matrix[i, k+1] = json["Data"][k][matrix[i, 0]].Value;             
            }
        }
        loadedNewJSON?.Invoke(); //Tell all other scripts that we updated the matrix
    }

    //This uses the JsonUtility to parse the JSON but for this to work I needed to change the JSON on runtime This is a simple way to solve this problem withought creating a full parser.
    public void ReadWithJSONUtility()
    {
        lastFileModify = File.GetLastWriteTime(filePath); //Save the last modification time of the file to make sure we are reading the file when it is modified

        bool fileLoaded = false;
        StreamReader reader = null;
        string testJson = "";
        while (!fileLoaded) //We keep trying to read the JSON file. This is important because the file is modified before it finishes saving and the other program can still be accessing it and it will return a error. 
        {
            try
            {
                reader = new StreamReader(filePath);
                testJson = reader.ReadToEnd();
                reader.Close();
                fileLoaded = true;
            }
            catch
            {
                fileLoaded = false;
            }
        }
        //Remove all elements from nested arrays and places them inside a single arrar of strings called data
        testJson = RemoveExtraCommas(testJson); // The test JSON was not valid because it had extra commas. This fixes it        
        string[] strs = testJson.Split(new string[] { "\"Data\" :" }, StringSplitOptions.None);
        strs[1] = ReplaceCharOutsideCommas(strs[1],':', ',');
        strs[1] = ReplaceCharOutsideCommas(strs[1], '{', ' ');
        strs[1] = ReplaceCharOutsideCommas(strs[1], '}', ' ');
        testJson = strs[0] + "\"Data\":" + strs[1] + '}';
        JSONOBJ myObject = JsonUtility.FromJson<JSONOBJ>(testJson); //Pasring the new json using JsonUtility
        
        title = myObject.Title;
        matrix = new string[myObject.ColumnHeaders.Length, (myObject.Data.Length/2)/myObject.ColumnHeaders.Length + 1]; //Creating our matrix array

        //Placing the headers acording to the ColumnHeaders
        for (int i = 0; i < myObject.ColumnHeaders.Length; i++)
        {
            matrix[i, 0] = myObject.ColumnHeaders[i];
        }

        int index = 1;
        //Placing the rows acording to the Data
        for (int k = 0; k < matrix.GetLength(1) - 1; k++)//Rows
        {
            for (int i = 0; i < matrix.GetLength(0); i++)//Columns
            {
                matrix[i, k + 1] = myObject.Data[index];
                index += 2;
            }
        }
        loadedNewJSON?.Invoke(); //Tell all other scripts that we updated the matrix
    }

    //Used to replace a char in a string checking if it is inside quatation marks
    string ReplaceCharOutsideCommas(string str, char replace, char with)
    {
        bool insideComma = false;
        string result = "";
        for (int i = 0; i < str.Length; i++)
        {
            if (str[i] == '"')
            {
                if(str[i-1] != '\\')
                {
                    insideComma = !insideComma;                    
                }
                result += str[i];
            }
            else if (str[i] == replace && !insideComma)
            {
                result += with;
            }
            else
            {
                result += str[i];
            }
        }
        return result;
    }

    //The test JSON was not valid because it had extra commas.This fixes it
    string RemoveExtraCommas(string str)
    {
        bool insideComma = false;
        string result = "";

        for (int i = 0; i < str.Length; i++)
        {
            if (str[i] == '"')
            {
                if (str[i - 1] != '\\')
                {
                    insideComma = !insideComma;
                }
                result += str[i];
            }
            else if (str[i] == ',' )
            {
                int count = 1;
                char nextValidChar  = str[i+ count];                
                while (nextValidChar == '\r' || nextValidChar == '\t' || nextValidChar == '\n' || nextValidChar == ' ' || nextValidChar.ToString() == "/n")
                {
                    count++;
                    nextValidChar = str[i + count];
                }
                if(nextValidChar != '}' && nextValidChar != ']')
                {
                    result += str[i];
                }
            }
            else
            {
                result += str[i];
            }
        }

        return result;
    }
}

[System.Serializable]
public class JSONOBJ
{
    public string Title;
    public string[] ColumnHeaders;
    public string[] Data;
}