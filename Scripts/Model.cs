[System.Serializable]
public class Model
{
    public string model { get; set; }
}

[System.Serializable]
public class ModelList
{
    public Model[] models;  // Lista de modelos
}