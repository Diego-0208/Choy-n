using UnityEngine;

[CreateAssetMenu(fileName = "Almanaque", menuName = "Scriptable Objects/Almanaque")]
public class Almanaque : ScriptableObject
{
    [Header("Identificación")]
    public string idPlanta; 

    [Header("Información General")]
    public Sprite img;
    public string nombre;

    [TextArea(3, 5)]
    public string desc;
    public string altura;

    [TextArea(3, 5)]
    public string cuidados;
}