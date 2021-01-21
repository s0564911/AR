using System.Collections;
using System.Linq;
using UnityEngine;

public class VuforiaTargetEvents : DefaultTrackableEventHandler
{
    public Controller controller;
    public Controller.Flower flower;

    protected override void OnTrackingFound()
    {
        base.OnTrackingFound();
        //controller.deactivateAllIcons();
        // controller._text.text = flower.display;
        controller.updateActive = true;
        controller.currentFlower = flower;
        flower.icons.transform.parent = transform;
        flower.icons.transform.position = flower.icons.transform.parent.position;
        
        // controller.btn_takeScreenshot.interactable = false;

        if (flower.rdy_icons)
        {
            flower.icons.all.SetActive(true);
        }
        else
            StartCoroutine(activateIcons());

        controller._outOf.text = flower.dataset.GetTrackables().Count() + "/5";
    }

    protected override void OnTrackingLost()
    {
        base.OnTrackingLost();
        
        // controller.btn_takeScreenshot.interactable = true;
        // controller.btn_addScreenshot.gameObject.SetActive(false);
        // controller.currentFlower = null;
        // controller._text.text = "";
        
        
        // controller._text.text = string.Empty;

        // if (!controller.addState)
        // {
        //     controller.btn_toggleAdd.gameObject.SetActive(false);
        // }
        // // controller.btn_addScreenshot.onClick.RemoveAllListeners();
        //
        // if (flower.rdy_icons)
        // {
        //     flower.icons.all.SetActive(false);
        //     flower.icons2?.all.SetActive(false);
        //     flower.icons3?.all.SetActive(false);
        //     flower.icons4?.all.SetActive(false);
        //     flower.icons5?.all.SetActive(false);
        // }
        
        // controller.currentFlower = null;
    }

    private IEnumerator activateIcons()
    {
        while (!flower.rdy_icons)
        {
            flower.icons.all.SetActive(true);
            yield return new WaitForEndOfFrame();
        }
        if (flower.rdy_icons)
            StopCoroutine(activateIcons());
    }
}