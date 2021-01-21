using UnityEngine;

public class Icons : MonoBehaviour
{
    public Controller controller;
    public GameObject all;

    public Controller.Flower flower;
    // public DateTime lastWatered;

    // sun
    public GameObject sun_full;
    public GameObject sun_partial;
    public GameObject sun_dappled;
    public GameObject sun_partShade;
    public GameObject sun_shade;

    // water
    public GameObject water_in;
    public GameObject water_wet;
    public GameObject water_mesic;
    public GameObject water_dry;

    // container
    public GameObject container_potted;
    public GameObject container_drainage;
    public GameObject container_large;
    public GameObject container_hanging;

    // Fruit
    public GameObject fruit_nut;
    public GameObject fruit_berries;
    public GameObject fruit_bean;

    // color
    public GameObject color_white;
    public GameObject color_orange;
    public GameObject color_purple;
    public GameObject color_violet;
    public GameObject color_red;
    public GameObject color_pink;
    public GameObject color_yellow;
    public GameObject color_blue;

    // Tox
    public GameObject tox;

    // lastWatered
    public GameObject none;
    public GameObject one;
    public GameObject two;
    public GameObject three;
    public GameObject four;
    public GameObject five;
    public GameObject six;
    public GameObject seven;
    public GameObject eight;
    public GameObject nine;
    public GameObject ten;
    public GameObject eleven;
    public GameObject twelve;
    public GameObject thirteen;
    public GameObject fourteen;
    public GameObject attention;
    
    // Animals
    public GameObject butterflies;

    private void OnMouseDown()
    {
        controller.resetDays(flower);
        controller.saveFlowers(controller._flowers, controller.file_path);

        // foreach (Icons icons in new[] {flower.icons, flower.icons2, flower.icons3, flower.icons4, flower.icons5})
        // {
        //     icons.one.SetActive(false);
        //     icons.two.SetActive(false);
        //     icons.three.SetActive(false);
        //     icons.four.SetActive(false);
        //     icons.five.SetActive(false);
        //     icons.six.SetActive(false);
        //     icons.seven.SetActive(false);
        //     icons.eight.SetActive(false);
        //     icons.nine.SetActive(false);
        //     icons.ten.SetActive(false);
        //     icons.eleven.SetActive(false);
        //     icons.twelve.SetActive(false);
        //     icons.thirteen.SetActive(false);
        //     icons.fourteen.SetActive(false);
        //     icons.attention.SetActive(false);
        //
        //     icons.none.SetActive(true);
        // }
        
            one.SetActive(false);
            two.SetActive(false);
            three.SetActive(false);
            four.SetActive(false);
            five.SetActive(false);
            six.SetActive(false);
            seven.SetActive(false);
            eight.SetActive(false);
            nine.SetActive(false);
            ten.SetActive(false);
            eleven.SetActive(false);
            twelve.SetActive(false);
            thirteen.SetActive(false);
            fourteen.SetActive(false);
            attention.SetActive(false);

            none.SetActive(true);
        
    }

    void Update()
    {
        gameObject.transform.LookAt(Vector3.zero);
        transform.rotation = Camera.main.transform.rotation;
        transform.position = gameObject.GetComponentInParent<Transform>().position;
    }

    private void OnBecameVisible()
    {
        controller.isVisible = true;
        controller.currentFlower = flower;
        controller._text.text = flower.display;

        controller.btn_takeScreenshot.interactable = false;
        if (string.IsNullOrEmpty(flower.guid5))
            controller.btn_toggleAdd.gameObject.SetActive(true);
    }

    private void OnBecameInvisible()
    {
        if (!controller.addState)
        {
            controller.isVisible = false;
            controller.currentFlower = null;
            
            if (controller._text.text != null)
                controller._text.text = string.Empty;

            controller.btn_takeScreenshot.interactable = true;
            // controller.btn_addScreenshot.gameObject.SetActive(false);
            controller.btn_toggleAdd.gameObject.SetActive(false);

            controller.addState = false;
            controller.updateActive = false;
            // gameObject.SetActive(false);}
        }
    }
}