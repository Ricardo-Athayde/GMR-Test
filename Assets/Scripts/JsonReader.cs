using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON; //Using a common JSON parser since Unity internal JsonUtility does not support nested arrays.
using System.IO;

public class JsonReader : MonoBehaviour
{
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
            ReadJson();
        }
    }

    public void ReadJson()
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
}