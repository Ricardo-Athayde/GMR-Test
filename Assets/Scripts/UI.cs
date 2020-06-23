using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    [SerializeField] GameObject TextPrefab = default; //Using a prefab as a base for the texts
    [SerializeField] Text title = default; //Title Text

    [Range(0,200)]
    [Tooltip("Default Font size for the cells")]
    [SerializeField] int textSize = default; //Default Font size
    [Range(0, 2)]
    [Tooltip("Multily the default font size for the headers")]
    [SerializeField] float headerSizeMultiplyer = default; //Multily the default font size for this

    [SerializeField] GridLayoutGroup layout = default; //The layout gropu used to organize the columns and rows

    // Start is called before the first frame update
    void Start()
    {
        JsonReader.self.loadedNewJSON += UpdateUI; //Subscribe to be notied when ther eis an update on the JSON
    }

    //Updates the UI
    void UpdateUI()
    {
        foreach(Transform obj in layout.transform) //Find all previous objects and destroy them before creating a new grid
        {
            Destroy(obj.gameObject);
        }

        title.text = JsonReader.self.title;
        layout.constraintCount = JsonReader.self.matrix.GetLength(0); //The grid layout is updated to have the correct number of columns
        for (int k = 0; k < JsonReader.self.matrix.GetLength(1); k++)
        {
            for (int i = 0; i < JsonReader.self.matrix.GetLength(0); i++)
            {
                Text txt = Instantiate(TextPrefab, layout.transform).GetComponent<Text>(); //Creates the text object
                txt.text = JsonReader.self.matrix[i, k];
                txt.fontSize = k != 0 ? textSize : (int)(textSize * headerSizeMultiplyer);
                txt.fontStyle = k != 0 ? FontStyle.Normal : FontStyle.Bold;
            }
        }
    }

    private void OnEnable()
    {
        if(JsonReader.self)
        {
            JsonReader.self.loadedNewJSON += UpdateUI;  //Subscribe to be notied when ther eis an update on the JSON
        }
    }

    private void OnDisable()
    {
        if (JsonReader.self)
        {
            JsonReader.self.loadedNewJSON -= UpdateUI; //Unsubscribe to be notied when ther eis an update on the JSON
        }
    }
}
