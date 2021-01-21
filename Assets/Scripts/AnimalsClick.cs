using UnityEngine;
using Random = UnityEngine.Random;

public class AnimalsClick : MonoBehaviour
{
    // Animal Colors
    public Material animal_white;
    public Material animal_orange;
    public Material animal_purple;
    public Material animal_violet;
    public Material animal_red;
    public Material animal_pink;
    public Material animal_yellow;
    public Material animal_blue;
    public Material animal_holoblue;

    public GameObject part1;
    public GameObject part2;
    public GameObject part3;


    private Material[] colors;
    private Material m_part1;
    private Material m_part2;
    private Material m_part3;
    
    private void Start()
    {
        colors = new[]
        {
            animal_white, 
            animal_orange, 
            animal_purple, 
            animal_violet, 
            animal_red, 
            animal_pink, 
            animal_yellow,
            animal_blue,
            animal_holoblue
        };

        // m_part1 = part1.GetComponent<Renderer>().material;
        // m_part2 = part2.GetComponent<Renderer>().material;
        // m_part3 = part3.GetComponent<Renderer>().material;
    }

    private void OnMouseDown()
    {
        int i = Random.Range(0, colors.Length - 1);
        part1.GetComponent<Renderer>().material = colors[i];
        part2.GetComponent<Renderer>().material = colors[i];
        part3.GetComponent<Renderer>().material = colors[i];
    }
}
