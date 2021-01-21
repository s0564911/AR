using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.UI;
using Vuforia;
using SimpleJSON;
using Image = UnityEngine.UI.Image;

public class Controller : MonoBehaviour
{
    #region Attributes

    // FINALS
    private const float targetWidth = 0.3f; // via cam
    private const string KEY = "2a10JVA8VQCOwcSGlUKPc1yfe";

    private const string URL = "https://my-api.plantnet.org/v2/identify/all?api-key=" + KEY;
    // private const string URL = "Test"; // enable for testing without api

    // Welcome
    public GameObject _welcome;
    public GameObject _blueBack;
    public bool _firstTime;
    public bool _firstTimeAdd;
    public bool _firstTimeJournal;
    public GameObject _intro;
    // public GameObject _introAdd;
    // public GameObject _introJournal;

    // Bottom
    public GameObject _bottom;
    public Button btn_takeScreenshot;
    public Button btn_addScreenshot;
    public Button btn_showFlowers;
    public Button btn_flash;
    public Text _count;
    private GameObject flashLight;
    private bool flashState;
    public Button btn_toggleAdd;
    public bool addState;

    // Overview
    public bool flowersOpen;
    public GameObject _backWork;
    public Button btn_flower1;
    public Button btn_flower2;
    public Button btn_flower3;
    public Button btn_flower4;
    public Button btn_flower5;
    public Button btn_flower6;
    public Button btn_nextPage;
    public Button btn_prvsPage;
    public Button btn_deleteTargets;

    // Details
    public bool detailsOpen;
    public GameObject _backDetails;
    public Button btn_closeDetails;
    public Button btn_nextDetails;
    public Button btn_prvsDetails;
    public Button btn_resetDays;
    public Button btn_deleteOne;
    public Button btn_addDetails;
    public Button btn_stopAddDetails;
    public bool addDetails;

    // Display Text
    public Text _text;
    public Text _outOf;

    public Text _lastWatered;
    // private Text _debug;


    // Data
    private bool searched_again;
    public string file_path;
    private JSONNode data;

    // AR
    private ObjectTracker _objectTracker;
    public List<Flower> _flowers;
    public Flower currentFlower;
    private GameObject _icons;

    // Interface Navigation
    public bool updateActive;
    public bool isVisible;
    private Image _image1;
    private Image _image2;
    private Image _image3;
    private Image _image4;
    private Image _image5;
    private Image _image6;
    private int _currentPage;
    private int _currentDetails;
    private Vector2 startPos;

    #endregion

    #region Start/Update

    void Start()
    {
        file_path = Application.persistentDataPath;

        btn_takeScreenshot.onClick.AddListener(takeScreenshot);
        btn_deleteTargets.onClick.AddListener(deleteFlowers);
        btn_showFlowers.onClick.AddListener(showFlowers);

        _text = GameObject.Find("DisplayText").GetComponent<Text>();
        // _debug = GameObject.Find("Debug").GetComponent<Text>();
        _icons = GameObject.Find("Icons").gameObject;
        _backWork = GameObject.Find("Back_Work").gameObject;
        _backDetails = GameObject.Find("Back_Details").gameObject;

        _objectTracker = TrackerManager.Instance.GetTracker<ObjectTracker>();
        // _objectTracker.Start();

        _image1 = btn_flower1.GetComponent<Image>();
        _image2 = btn_flower2.GetComponent<Image>();
        _image3 = btn_flower3.GetComponent<Image>();
        _image4 = btn_flower4.GetComponent<Image>();
        _image5 = btn_flower5.GetComponent<Image>();
        _image6 = btn_flower6.GetComponent<Image>();

        btn_flower1.onClick.AddListener(delegate { openDetails(1, false, null); });
        btn_flower2.onClick.AddListener(delegate { openDetails(2, false, null); });
        btn_flower3.onClick.AddListener(delegate { openDetails(3, false, null); });
        btn_flower4.onClick.AddListener(delegate { openDetails(4, false, null); });
        btn_flower5.onClick.AddListener(delegate { openDetails(5, false, null); });
        btn_flower6.onClick.AddListener(delegate { openDetails(6, false, null); });

        btn_nextPage.onClick.AddListener(nextPage);
        btn_prvsPage.onClick.AddListener(prvsPage);
        btn_closeDetails.onClick.AddListener(closeDetails);
        btn_nextDetails.onClick.AddListener(nextDetails);
        btn_prvsDetails.onClick.AddListener(prvsDetails);
        btn_flash.onClick.AddListener(toggleFlash);

        flashLight = btn_flash.transform.Find("Light").gameObject;

        btn_toggleAdd.onClick.AddListener(toggleAdd);
        // btn_toggleAdd.onClick.AddListener(delegate { openDetails(0, true, currentFlower); }); // test swipe up into details

        StartCoroutine(loadFlowers(file_path));

        CameraDevice.Instance.SetFocusMode(
            CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO);
    }

    void Update()
    {
        // if (detailsOpen || flowersOpen && !addState)
        // {
        //     btn_addScreenshot.gameObject.SetActive(false);
        //     btn_takeScreenshot.interactable = false;
        // }
        // else if (!addDetails && !addState)
        // {
        //     _objectTracker?.Start();
        // }
        if (!detailsOpen && !flowersOpen && currentFlower == null && string.IsNullOrEmpty(_text.text))
        {
            btn_takeScreenshot.interactable = true;
            btn_toggleAdd.gameObject.SetActive(false);
        }

        if (addState)
        {
            btn_toggleAdd.gameObject.SetActive(false);
        }

        swipe();
    }

    #endregion

    #region Interface

    #region Common

    private void swipe()
    {
        const int minSwipeDist = 50;

        if (!addDetails && !addState && Input.touchCount > 0)

        {
            Touch touch = Input.touches[0];
            switch (touch.phase)

            {
                case TouchPhase.Began:

                    startPos = touch.position;
                    break;
                case TouchPhase.Ended:
                    float swipeDistY =
                        (new Vector3(0, touch.position.y, 0) - new Vector3(0, startPos.y, 0)).magnitude;
                    float swipeDistX =
                        (new Vector3(touch.position.x, 0, 0) - new Vector3(startPos.x, 0, 0)).magnitude;

                    if (swipeDistY > minSwipeDist && swipeDistY > swipeDistX)
                    {
                        float swipeValue = Mathf.Sign(touch.position.y - startPos.y);
                        //
                        if (swipeValue < 0 && _backWork.transform.GetChild(0).gameObject.activeSelf)
                        {
                            //down swipe
                            _objectTracker.Start();
                            showFlowers();
                        }
                        else if (swipeValue > 0 && !_backWork.transform.GetChild(0).gameObject.activeSelf)
                        {
                            //up swipe
                            if (!addState && !string.IsNullOrEmpty(_text.text) && currentFlower != null)
                            {
                                showFlowers();
                                openDetails(0, true, currentFlower);
                            }
                            else if (!addState)
                                showFlowers();
                        }
                    }
                    else if (swipeDistX > minSwipeDist)
                    {
                        // flowers
                        float swipeValue = Mathf.Sign(touch.position.x - startPos.x);
                        if (flowersOpen && !detailsOpen)
                        {
                            if (swipeValue < 0)
                            {
                                // right swipe
                                nextPage();
                            }
                            else if (swipeValue > 0)
                            {
                                // left swipe
                                prvsPage();
                            }
                        }
                        else if (detailsOpen)
                        {
                            if (swipeValue < 0)
                            {
                                // right swipe
                                nextDetails();
                            }
                            else if (swipeValue > 0 && btn_prvsDetails.gameObject.activeSelf)
                            {
                                // left swipe
                                prvsDetails();
                            }
                        }

                        // details
                    }


                    break;
            }
        }
    }

    private void stopAddScreenshot()
    {
        addDetails = false;
        addState = false;
        _text.text = string.Empty;
        btn_takeScreenshot.interactable = true;
        _objectTracker.Start();

        btn_showFlowers.gameObject.SetActive(true);
        btn_takeScreenshot.gameObject.SetActive(true);
        btn_addScreenshot.gameObject.SetActive(false);
        btn_addScreenshot.onClick.RemoveAllListeners();
        btn_stopAddDetails.onClick.RemoveAllListeners();
        btn_stopAddDetails.gameObject.SetActive(false);
    }

    #endregion

    #region Bottom

    private void toggleFlash()
    {
        if (flashState == false)
        {
            CameraDevice.Instance.SetFlashTorchMode(true);
            flashState = true;
        }
        else
        {
            CameraDevice.Instance.SetFlashTorchMode(false);
            flashState = false;
        }

        flashLight.SetActive(flashState);
    }

    private void toggleAdd()
    {
        addState = true;
        _objectTracker.Stop();
        // _text.text = currentFlower.display;
        btn_showFlowers.gameObject.SetActive(false);
        btn_stopAddDetails.gameObject.SetActive(true);
        btn_addScreenshot.gameObject.SetActive(true);
        btn_takeScreenshot.gameObject.SetActive(false);
        btn_toggleAdd.gameObject.SetActive(false);
        btn_stopAddDetails.onClick.AddListener(stopAddScreenshot);
        btn_addScreenshot.onClick.RemoveAllListeners();
        btn_addScreenshot.onClick.AddListener(delegate { addScreenshot(currentFlower); });
        
        if (!_firstTimeAdd)
        {
            // _introAdd.SetActive(true);
            _firstTimeAdd = true;
        }
    }

    #endregion

    #region Flowers

    void showFlowers()
    {
        if (_backWork.transform.GetChild(0).gameObject.activeSelf ||
            _backDetails.transform.GetChild(0).gameObject.activeSelf)
        {
            flowersOpen = false;
            detailsOpen = false;
            btn_takeScreenshot.interactable = true;
            if (!_objectTracker.IsActive)
                _objectTracker.Start();
        }
        else
        {
            flowersOpen = true;
            btn_takeScreenshot.interactable = false;
        }

        updatePageNavigation();

        foreach (Transform child in _backWork.transform)
        {
            if (null == child)
                continue;

            child.gameObject.SetActive(!child.gameObject.activeSelf);
        }

        foreach (Transform child in _backDetails.transform)
        {
            if (null == child)
                continue;

            if (child.gameObject.activeSelf)
                child.gameObject.SetActive(false);
        }
        
        if (!_firstTimeJournal)
        {
            // _introJournal.SetActive(true);
            _firstTimeJournal = true;
        }
    }

    void nextPage()
    {
        _currentPage++;
        updateView();
    }

    void prvsPage()
    {
        if (_currentPage > 0)
        {
            _currentPage--;
            updateView();
        }
    }

    void updatePageNavigation()
    {
        int lastPage = (_flowers.Count - 1) / 6;
        btn_prvsPage.gameObject.SetActive(_currentPage > 0 && _flowers.Count > 6);
        bool test = _currentPage != lastPage;
        btn_nextPage.gameObject.SetActive(test);
        if (_currentPage > lastPage)
        {
            _currentPage = lastPage;
            updateView();
        }
    }

    void updateView()
    {
        updatePageNavigation();
        Image[] images = {_image1, _image2, _image3, _image4, _image5, _image6};
        foreach (Image image in images)
        {
            // reset alpha
            Color c = image.color;
            c.a = 0;
            image.color = c;
        }

        int flowers_count = _flowers.Count;
        int image_count = 0;
        if (flowers_count > 6 * _currentPage && _currentPage != 0)
            flowers_count = 6 * (_currentPage + 1);
        for (int i = 1 + _currentPage * 6; i <= 1 + flowers_count; i++)
        {
            try
            {
                if (image_count < 6 && _flowers[i - 1] != null)
                    setSprite(_flowers[i - 1], i - _currentPage * 6);

                int index = i - 1 - _currentPage * 6;
                if (index < 6 && index >= 0)
                {
                    Image image = images[index];
                    // set alpha
                    Color c = image.color;
                    c.a = 1;
                    image.color = c;
                }
            }
            catch (Exception)
            {
                // _debug.text = e.StackTrace;
            }

            image_count++;
        }
    }

    #endregion

    #region Details

    void openDetails(int i, bool direct, Flower flower)
    {
        detailsOpen = true;

        if (addState)
        {
            addState = false;
            btn_toggleAdd.gameObject.SetActive(false);
        }

        currentFlower = null;
        detailsOpen = true;

        int y = i;
        // if (_currentPage == 0)
        //     updateView();
        if (!direct)
        {
            y--;
            _currentDetails = y;
            y += _currentPage * 6;

            if (_flowers.Count <= y)
                return;

            foreach (Transform child in _backDetails.transform)
            {
                if (null == child)
                    continue;
                child.gameObject.SetActive(!child.gameObject.activeSelf);
                // showFlowers(child.gameObject);
            }
        }
        else
        {
            if (_flowers.Count <= y)
                return;
            if (flower != null)
            {
                for (int j = 0; j < _flowers.Count; j++)
                {
                    if (flower.display == _flowers[j].display)
                    {
                        y = j;
                        break;
                    }
                }
                // y = _flowers.IndexOf(flower);

                foreach (Transform child in _backDetails.transform)
                {
                    if (null == child)
                        continue;
                    child.gameObject.SetActive(!child.gameObject.activeSelf);
                    // showFlowers(child.gameObject);
                }
            }

            _currentDetails = y;
        }

        // if (flower != null)
        //     y = _flowers.IndexOf(flower);

        btn_addDetails.gameObject.SetActive(false);

        updateDetailsNavigation(_flowers[y]);

        if (!_flowers[y].display.Equals(string.Empty) && !_flowers[y].display.Equals("Debug"))
        {
            _backDetails.transform.Find("Title").GetComponent<Text>().text =
                _flowers[y].display.Replace("/", " ") +
                Environment.NewLine + Environment.NewLine +
                _flowers[y].accuracy.ToString(CultureInfo.InvariantCulture).Substring(2, 2) + "% sure";

            if (_flowers[y].display != _flowers[y].latin)
                _backDetails.transform.Find("Latin").GetComponent<Text>().text = _flowers[y].latin;
        }
        else if (_flowers[y].display.Contains("Debug"))
        {
            _backDetails.transform.Find("Title").GetComponent<Text>().text = "Debug " + y; // :) 
            _backDetails.transform.Find("Latin").GetComponent<Text>().text =
                "Trackables: " + _flowers[y].dataset.GetTrackables().Count();
        }
        else
        {
            _backDetails.transform.Find("Title").GetComponent<Text>().text = _flowers[y].latin;
        }

        _backDetails.transform.Find("Details").GetComponent<Text>().text = _flowers[y].details;

        Sprite sprite = LoadNewSprite(_flowers[y].small_path);
        Image target = _backDetails.transform.Find("Flower").GetComponent<Image>();
        target.overrideSprite = sprite;

        if (_flowers[y].dataset.GetTrackables().Count() < 5)
        {
            btn_addDetails.onClick.AddListener(delegate { addDetailsScreenshot(_flowers[y]); });
            btn_addDetails.gameObject.SetActive(true);
        }

        // btn_addDetails.onClick.AddListener(delegate { addDetailsScreenshot(_flowers[i]); });
        btn_deleteOne.onClick.AddListener(delegate { deleteOne(_flowers[y]); });

        if (_flowers[y].lastWatered != null)
            _lastWatered.text = (DateTime.Now - Convert.ToDateTime(_flowers[y].lastWatered) - TimeSpan.FromDays(1)).Days
                .ToString();
        else
        {
            // update old entries
            _lastWatered.text = "2";
            _flowers[y].lastWatered = (DateTime.Now - TimeSpan.FromDays(2)).ToString(CultureInfo.InvariantCulture);
            saveFlowers(_flowers, file_path);
        }

        // ToDo
        updateView();
        _objectTracker.Stop();

        btn_resetDays.onClick.RemoveAllListeners();
        btn_resetDays.onClick.AddListener(delegate { resetDays(_flowers[y]); });
    }

    void nextDetails()
    {
        openDetails(_currentDetails + 6 * _currentPage + 1, true, null);
    }

    void prvsDetails()
    {
        openDetails(_currentDetails + 6 * _currentPage - 1, true, null);
    }

    void closeDetails()
    {
        detailsOpen = false;

        if (!_backWork.transform.GetChild(0).gameObject.activeSelf)
        {
            _objectTracker.Start();
        }

        foreach (Transform child in _backDetails.transform)
        {
            if (null == child)
                continue;
            child.gameObject.SetActive(false);
        }

        btn_addDetails.onClick.RemoveAllListeners();
        btn_deleteOne.onClick.RemoveAllListeners();
        btn_resetDays.onClick.RemoveAllListeners();
    }

    void updateDetailsNavigation(Flower flower)
    {
        btn_prvsDetails.gameObject.SetActive(_flowers.IndexOf(flower) != 0);
        btn_nextDetails.gameObject.SetActive(_flowers.IndexOf(flower) != _flowers.Count - 1);
    }

    public void resetDays(Flower flower)
    {
        flower.lastWatered = DateTime.Now.ToString(CultureInfo.InvariantCulture);
        _lastWatered.text = 0.ToString();
        saveFlowers(_flowers, file_path);
    }

    private void addDetailsScreenshot(Flower flower)
    {
        _text.text = flower.display;
        _outOf.text = flower.dataset.GetTrackables().Count() + "/5";
        addDetails = true;
        _objectTracker.Stop();

        btn_takeScreenshot.gameObject.SetActive(false);
        btn_addScreenshot.gameObject.SetActive(true);
        btn_showFlowers.gameObject.SetActive(false);

        btn_addScreenshot.onClick.RemoveAllListeners();
        btn_addScreenshot.onClick.AddListener(delegate { addScreenshot(flower); });

        btn_stopAddDetails.gameObject.SetActive(true);
        btn_stopAddDetails.onClick.AddListener(stopAddScreenshot);

        closeDetails();
        foreach (Transform child in _backWork.transform)
        {
            if (null == child)
                continue;

            child.gameObject.SetActive(false);
        }
        
        if (!_firstTimeAdd)
        {
            // _introAdd.SetActive(true);
            _firstTimeAdd = true;
        }
    }

    #endregion

    #endregion

    #region AR/Vuforia

    #region AddScreenshot

    private void addScreenshot(Flower flower)
    {
        // if (addDetails)
        //     stopDetailsScreenshot();
        // if (!addDetails)
        //     btn_addScreenshot.onClick.RemoveAllListeners();

        _outOf.text = flower.dataset.GetTrackables().Count() + "/5";

        string new_guid = Guid.NewGuid().ToString();
        string screen_path = file_path + "/" + new_guid + ".png";
        if (flower.screen_path_2 != null && flower.screen_path_2.Equals(string.Empty))
        {
            flower.screen_path_2 = screen_path;
            flower.guid2 = new_guid;
        }
        else if (flower.screen_path_3 != null && flower.screen_path_3.Equals(string.Empty))
        {
            flower.screen_path_3 = screen_path;
            flower.guid3 = new_guid;
        }
        else if (flower.screen_path_4 != null && flower.screen_path_4.Equals(string.Empty))
        {
            flower.screen_path_4 = screen_path;
            flower.guid4 = new_guid;
        }
        else if (flower.screen_path_5 != null && flower.screen_path_5.Equals(string.Empty))
        {
            flower.screen_path_5 = screen_path;
            flower.guid5 = new_guid;
        }

        StartCoroutine(AddScreenshot(flower, screen_path, new_guid));
    }

    private IEnumerator AddScreenshot(Flower flower, string screen_path, string guid)
    {
        yield return new WaitForEndOfFrame();

        int width = Screen.width;
        int height = Screen.height;
        // Creates a new texture of the size of screen.
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        // Reads every pixel displayed on the screen.
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();

        // Resize so Vuforia can create a marker.
        tex = CropScale.CropTexture(tex, new Vector2(width, height * 3 / 4), CropOptions.TOP_LEFT);
        Texture2D texScaled = Resize(tex, 800, 1200); // ~

        // Save the screenshot as a byte array.
        byte[] bytes = texScaled.EncodeToPNG();
        // flower.tex = tex;
        flower.texSmall = CropScale.CropTexture(
            tex, //CropScale.ScaleTexture(tex, 600, tex.height / (tex.width / 600)),
            new Vector2(600, 600));
        Destroy(tex);

        // Save the screenshot on disk. (for marker and later use, loading)
        File.WriteAllBytes(screen_path, bytes);

        yield return new WaitForEndOfFrame();

        int index = 2;
        if (flower.screen_path_3 == null)
            index = 3;
        else if (flower.screen_path_4 == null)
            index = 4;
        else if (flower.screen_path_5 == null)
            index = 5;
        AddMarker(flower, screen_path, guid);
    }

    void AddMarker(Flower flower, string screen_path, string guid)
    {
        // get the runtime image source and set the texture to load
        var runtimeImageSource = _objectTracker.RuntimeImageSource;

        // _text.text = File.Exists(screen_path) ? "Found" : "Not found";

        var imageSourceIsLoaded = runtimeImageSource.SetFile(VuforiaUnity.StorageType.STORAGE_ABSOLUTE,
            screen_path,
            targetWidth,
            guid);

        if (imageSourceIsLoaded)
        {
            // deactivate the dataset
            _objectTracker.DeactivateDataSet(flower.dataset);
            var trackableBehaviour = flower.dataset.CreateTrackable(runtimeImageSource, guid);

            // add the DefaultTrackableEventHandler to the readily existing flower
            GameObject o;
            (o = trackableBehaviour.gameObject).AddComponent<DefaultTrackableEventHandler>();
            o.GetComponent<DefaultTrackableEventHandler>().StatusFilter = DefaultTrackableEventHandler
                .TrackingStatusFilter.Tracked_ExtendedTracked; // ToDo
            o.transform.parent = GameObject.Find(flower.guid).transform;

            _objectTracker.ActivateDataSet(flower.dataset);

            // setup target events
            VuforiaTargetEvents events = o.AddComponent<VuforiaTargetEvents>();
            events.controller = this;
            events.flower = flower;
            events.StatusFilter = DefaultTrackableEventHandler.TrackingStatusFilter.Tracked_ExtendedTracked;

            int count = flower.dataset.GetTrackables().Count();
            _outOf.text = count + "/5";

            // flower.icons.all.SetActive(true);
        }

        // if (direct)
        saveFlowers(_flowers, file_path);
    }

    #endregion

    #region TakeScreenshot

    void takeScreenshot()
    {
        string new_guid = Guid.NewGuid().ToString();
        string screen_path = file_path + "/" + new_guid + ".png";
        string small_path = file_path + "/" + new_guid + "_small.png";
        Flower flower = new Flower
        {
            guid = new_guid,
            screen_path = screen_path,
            small_path = small_path,
            lastWatered = DateTime.Now.ToString(CultureInfo.InvariantCulture)
        };
        // _debug.text = _debug.text + Environment.NewLine + "Set (" + screen_path + ") as Path!";

        StartCoroutine(TakeScreenshot(flower));
    }

    private IEnumerator TakeScreenshot(Flower flower)
    {
        yield return new WaitForEndOfFrame();

        int width = Screen.width;
        int height = Screen.height;
        // Creates a new texture of the size of screen.
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        // Reads every pixel displayed on the screen.
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();

        // Upload image to API as byte array.
        byte[] bytes = tex.EncodeToPNG();
        StartCoroutine(UploadImage(URL, bytes, flower));

        // Resize so Vuforia can create a marker.
        tex = CropScale.CropTexture(tex, new Vector2(width, height * 3 / 4), CropOptions.TOP_LEFT);
        Texture2D texScaled = Resize(tex, 800, 1200); // ~

        // Save the screenshot as a byte array.
        bytes = texScaled.EncodeToPNG();
        // flower.tex = tex;
        flower.texSmall = CropScale.CropTexture(
            tex, //CropScale.ScaleTexture(tex, 600, tex.height / (tex.width / 600)), //
            new Vector2(600, 600));
        Destroy(tex);
        // Destroy(texScaled);

        // Save the screenshot on disk. (for marker and later use)
        File.WriteAllBytes(flower.screen_path, bytes);
        File.WriteAllBytes(flower.small_path, flower.texSmall.EncodeToPNG());
        // _debug.text = _debug.text + Environment.NewLine + "Screenshot (" + screen_path + ") saved!";
    }

    private IEnumerator UploadImage(string url, byte[] bodyRaw, Flower flower)
    {
        WWWForm formrequest = new WWWForm();
        formrequest.AddBinaryData("images", bodyRaw, "request.png", "image/png");
        formrequest.AddField("organs", "leaf");

        using (UnityWebRequest www = UnityWebRequest.Post(url, formrequest))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError)
            {
                if (URL == "Test")
                {
                    flower.display = "Debug";
                    flower.accuracy = 0;
                    flower.latin = "Testare";


                    if (_flowers == null)
                        _flowers = new List<Flower>();
                    _flowers.Add(flower);
                    _count.text = _flowers.Count.ToString();
                    saveFlowers(_flowers, file_path);
                    CreateMarker(flower, true);
                }
                else
                    _text.text = "Error API";
            }
            else
            {
                data = JSON.Parse(www.downloadHandler.text);

                try
                {
                    flower.display = data[3][0][1][4][0].Value;
                    flower.accuracy = float.Parse(data[3][0][0].Value);
                    flower.latin = data[3][0][1][0].Value;

                    if (flower.display == null)
                        flower.display = flower.latin;
                    else if (flower.display.Contains("/"))
                        flower.display = flower.display.Replace("/", " ");

                    // _text.text = flower.display;
                    // _debug.text = flower.accuracy + " | " + flower.latin + " | " + flower.display;

                    if (_flowers == null)
                        _flowers = new List<Flower>();
                    _flowers.Add(flower);
                    _count.text = _flowers.Count.ToString();
                    saveFlowers(_flowers, file_path);

                    flower.display = flower.display.Equals(string.Empty) ? flower.latin : flower.display;

                    CreateMarker(flower, true);
                }
                catch (Exception)
                {
                    _text.text = www.responseCode.Equals(404) ? "try again" : www.responseCode.ToString();
                }
            }
        }
    }

    private static Texture2D Resize(Texture2D source, int newWidth, int newHeight)
    {
        // cheers : https://gamedev.stackexchange.com/questions/92285/unity3d-resize-texture-without-corruption
        source.filterMode = FilterMode.Point;
        RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
        rt.filterMode = FilterMode.Point;
        RenderTexture.active = rt;
        Graphics.Blit(source, rt);
        Texture2D nTex = new Texture2D(newWidth, newHeight);
        nTex.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
        nTex.Apply();
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);
        return nTex;
    }

    void CreateMarker(Flower flower, bool update)
    {
        // get the runtime image source and set the texture to load
        var runtimeImageSource = _objectTracker.RuntimeImageSource;

        // _text.text = File.Exists(screen_path) ? "Found" : "Not found";

        var imageSourceIsLoaded = runtimeImageSource.SetFile(VuforiaUnity.StorageType.STORAGE_ABSOLUTE,
            flower.screen_path,
            targetWidth,
            flower.guid);

        // create a new dataset and use the source to create a new trackable
        if (flower.dataset == null)
        {
            var dataset = _objectTracker.CreateDataSet();
            flower.dataset = dataset;
        }

        if (imageSourceIsLoaded)
        {
            var trackableBehaviour = flower.dataset.CreateTrackable(runtimeImageSource, flower.guid);

            // flower.target = trackableBehaviour.gameObject;

            // add the DefaultTrackableEventHandler to the newly created flower
            GameObject o;
            (o = trackableBehaviour.gameObject).AddComponent<DefaultTrackableEventHandler>();
            o.transform.tag = "Target";
            o.GetComponent<DefaultTrackableEventHandler>().StatusFilter = DefaultTrackableEventHandler
                .TrackingStatusFilter.Tracked_ExtendedTracked; //ToDo
            // o.AddComponent<FaceCamera>();


            VuforiaTargetEvents events = o.AddComponent<VuforiaTargetEvents>();
            events.controller = this;
            events.flower = flower;
            events.StatusFilter = DefaultTrackableEventHandler.TrackingStatusFilter.Tracked_ExtendedTracked;

            flower.trackable = o;

            // _debug.text = screen_path + " created!";
        }
        else
        {
            // _debug.text = _debug.text + Environment.NewLine + "Loading " + flower.guid + " failed!";
            // return null;
        }

        // activate the dataset
        // _objectTracker.Stop(); //~
        _objectTracker.ActivateDataSet(flower.dataset);
        // _objectTracker.Start(); //~

        // add virtual content as child object(s)
        // ToDo ... serialize right!
        if (update)
        {
            StartCoroutine(getData(flower, flower.latin, update));
        }
        else
        {
            // finalizeLoading(flower);
            StartCoroutine(getData(flower, flower.latin, update));
        }
    }

    #endregion

    #endregion

    #region Data

    private IEnumerator getData(Flower flower, string search_term, bool update)
    {
        // string search_term = flower.latin; // "Pilea+peperomioides";
        // search_term = "Castanospermum australe";
        using (UnityWebRequest www = UnityWebRequest.Get("https://garden.org/plants/search/text/?q=" + search_term))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError)
            {
                _text.text = "Error DATA 1";
            }
            else
            {
                string html_raw = www.downloadHandler.text;
                try
                {
                    string hit =
                        html_raw.Split(new[] {"table table-striped table-bordered table-hover pretty-table"},
                                StringSplitOptions.RemoveEmptyEntries)[1]
                            .Split(new[] {"href=\"", "\"/>"}, StringSplitOptions.RemoveEmptyEntries)[1]
                            .Split(new[] {"\"><"}, StringSplitOptions.RemoveEmptyEntries)[0];

                    StartCoroutine(getTables(flower, hit, update));
                }
                catch (Exception)
                {
                    flower.details = "Debug Mode";
                    StartCoroutine(finalizeLoading(flower, update));
                }
            }
        }
    }

    private IEnumerator getTables(Flower flower, string hit, bool update)
    {
        using (UnityWebRequest www = UnityWebRequest.Get("https://garden.org" + hit))
        {
            yield return www.SendWebRequest();

            if (www.isNetworkError)
            {
                _text.text = "Error DATA 2";
            }
            else
            {
                string html_raw = www.downloadHandler.text;
                string[] tables_raw = html_raw.Split(
                    new[] {"table table-striped table-bordered table-hover simple-table"},
                    StringSplitOptions.RemoveEmptyEntries);
                string info = string.Empty;
                // string specific_data;

                if (tables_raw.Length == 2)
                {
                    info = tables_raw[1];
                }

                if (tables_raw.Length > 2)
                {
                    // specific_data = tables_raw[1];
                    info = tables_raw[2];
                }

                string[] infos = info.Split(new[] {"<tr>", "</tr>"}, StringSplitOptions.RemoveEmptyEntries);

                flower.details = string.Empty;

                foreach (string s in infos)
                {
                    string[] keys = s.Split(new[] {">", ":<"}, StringSplitOptions.RemoveEmptyEntries);
                    string key = string.Empty;

                    if (keys.Length > 1)
                        key = s.Split(new[] {">", ":<"}, StringSplitOptions.RemoveEmptyEntries)[1];
                    else
                        yield return null;

                    string[] values = s.Split(new[] {">", "<BR>"}, StringSplitOptions.RemoveEmptyEntries);
                    string value = string.Empty;

                    if (values.Length >= 2)
                        value = values[values.Length - 2];

                    if (values.Length >= 3 && values[values.Length - 3].Contains("</span"))
                    {
                        value = value + "/" + values[values.Length - 3]
                            .Split(new[] {"</span"}, StringSplitOptions.RemoveEmptyEntries)[0];
                    }

                    string[] target_keys =
                    {
                        "Plant Habit",
                        "Sun Requirements",
                        "Water Preferences",
                        "Containers",
                        "Suitable Locations",
                        "Resistances",
                        "Propagation",
                        "Wildlife Attractant",
                        "Toxicity",
                        "Pollinators",
                        "Minimum cold hardiness",
                        "Leaves",
                        "Flowers",
                        "Flower Color",
                        "Bloom Size",
                        "Underground structures",
                        "Life cycle",

                        "Plant Height",
                        "Plant Spread",
                        "Fruit",
                        "Flower Time",
                        "Uses",
                        "Dynamic Accumulator"
                    };

                    if (target_keys.Contains(key))
                    {
                        if (value.Contains("</td") && values.Length > 2)
                        {
                            value = values[values.Length - 3];
                            // value = value.Split(new[] {"</td"}, StringSplitOptions.RemoveEmptyEntries)[0];
                        }

                        if (value.Contains("\n"))
                            value = value.Split(new[] {"\n"}, StringSplitOptions.RemoveEmptyEntries)[0];
                        if (value.Contains("</span"))
                            value = value.Split(new[] {"</span"}, StringSplitOptions.RemoveEmptyEntries)[0];
                        if (value.Contains("Other: "))
                            value = value.Split(new[] {"Other: "}, StringSplitOptions.RemoveEmptyEntries)[0];

                        flower.details = flower.details + key + ": " + value + Environment.NewLine;
                        flower.atts.Add(key, value);
                    }
                }

                if (flower.details.Equals(string.Empty) && !searched_again)
                {
                    searched_again = true;
                    StartCoroutine(getData(flower,
                        flower.latin.Split(new[] {" "}, StringSplitOptions.RemoveEmptyEntries)[0], update));
                }

                searched_again = false;

                if (flower.lastWatered.Equals(DateTime.MinValue.ToString(CultureInfo.InvariantCulture)))
                    flower.lastWatered = DateTime.Today.Subtract(new TimeSpan(5, 0, 0, 0))
                        .ToString(CultureInfo.InvariantCulture);

                StartCoroutine(finalizeLoading(flower, update));
            }
        }
    }

    #endregion

    #region Save/Load/Delete

    private IEnumerator finalizeLoading(Flower flower, bool update)
    {
        yield return new WaitForEndOfFrame();

        if (_flowers.Count < 7)
            setSprite(flower, _flowers.Count);

        _icons.GetComponent<Icons>().flower = flower;

        setUpIcons(Instantiate(_icons, GameObject.Find(flower.guid).transform, false).GetComponent<Icons>(), flower,
            update);

        if (flower.screen_path_2 != null && !flower.screen_path_2.Equals(string.Empty))
        {
            yield return new WaitForEndOfFrame();
            AddMarker(flower, flower.screen_path_2, flower.guid2);
        }

        if (flower.screen_path_3 != null && !flower.screen_path_3.Equals(string.Empty))
        {
            yield return new WaitForEndOfFrame();
            AddMarker(flower, flower.screen_path_3, flower.guid3);
        }

        if (flower.screen_path_4 != null && !flower.screen_path_4.Equals(string.Empty))
        {
            yield return new WaitForEndOfFrame();
            AddMarker(flower, flower.screen_path_4, flower.guid4);
        }

        if (flower.screen_path_5 != null && !flower.screen_path_5.Equals(string.Empty))
        {
            yield return new WaitForEndOfFrame();
            AddMarker(flower, flower.screen_path_5, flower.guid5);
        }
    }

    private static void setUpIcons(Icons icons, Flower flower, bool update)
    {
        // lastWatered
        icons.flower = flower;
        flower.icons = icons;
        int diff = (DateTime.Now - Convert.ToDateTime(flower.lastWatered)).Days;
        if (diff < 1)
            icons.none.SetActive(true);
        else if (diff > 1 && diff <= 2)
            icons.one.SetActive(true);
        else if (diff > 2 && diff <= 3)
            icons.two.SetActive(true);
        else if (diff > 3 && diff <= 4)
            icons.thirteen.SetActive(true);
        else if (diff > 4 && diff <= 5)
            icons.four.SetActive(true);
        else if (diff > 5 && diff <= 6)
            icons.five.SetActive(true);
        else if (diff > 6 && diff <= 7)
            icons.six.SetActive(true);
        else if (diff > 7 && diff <= 8)
            icons.seven.SetActive(true);
        else if (diff > 8 && diff <= 9)
            icons.eight.SetActive(true);
        else if (diff > 9 && diff <= 10)
            icons.nine.SetActive(true);
        else if (diff > 10 && diff <= 11)
            icons.ten.SetActive(true);
        else if (diff > 11 && diff <= 12)
            icons.eleven.SetActive(true);
        else if (diff > 12 && diff <= 13)
            icons.twelve.SetActive(true);
        else if (diff > 13 && diff <= 14)
            icons.thirteen.SetActive(true);
        else if (diff > 14 && diff <= 15)
            icons.four.SetActive(true);
        else if (diff > 15)
            icons.attention.SetActive(true);

        if (flower.atts.Keys.Contains("Sun Requirements"))
        {
            if (flower.atts["Sun Requirements"].Contains("Full Sun"))
            {
                icons.sun_full.SetActive(true);
            }
            else if (flower.atts["Sun Requirements"].Contains("Partial Sun"))
            {
                icons.sun_partial.SetActive(true);
            }
            else if (flower.atts["Sun Requirements"].Contains("Dappled Shade"))
            {
                icons.sun_dappled.SetActive(true);
            }
            else if (flower.atts["Sun Requirements"].Contains("Partial Shade"))
            {
                icons.sun_partShade.SetActive(true);
            }
            else if (flower.atts["Sun Requirements"].Contains("Full Shade"))
            {
                icons.sun_shade.SetActive(true);
            }
        }

        if (flower.atts.Keys.Contains("Water Preferences"))
        {
            if (flower.atts["Water Preferences"].Contains("In Water"))
            {
                icons.water_in.SetActive(true);
                icons.water_mesic.SetActive(true);
                icons.water_wet.SetActive(true);
                icons.water_dry.SetActive(true);
            }
            else if (flower.atts["Water Preferences"].Contains("Wet"))
            {
                icons.water_in.SetActive(false);
                icons.water_mesic.SetActive(true);
                icons.water_wet.SetActive(true);
                icons.water_dry.SetActive(true);
            }
            else if (flower.atts["Water Preferences"].Contains("Dry") || flower.atts.Keys.Contains("Resistances") &&
                flower.atts["Resistances"].ToLower()
                    .Contains("draught"))
            {
                icons.water_in.SetActive(false);
                icons.water_mesic.SetActive(false);
                icons.water_wet.SetActive(false);
                icons.water_dry.SetActive(true);
            }
            else
            {
                icons.water_in.SetActive(false);
                icons.water_mesic.SetActive(false);
                icons.water_wet.SetActive(true);
                icons.water_dry.SetActive(true);
            }
        }
        else
        {
            icons.water_in.SetActive(false);
            icons.water_mesic.SetActive(false);
            icons.water_wet.SetActive(true);
            icons.water_dry.SetActive(true);
        }

        if (flower.atts.Keys.Contains("Containers"))
        {
            if (flower.atts["Containers"].Contains("potted"))
            {
                icons.container_potted.SetActive(true);
            }
            else if (flower.atts["Containers"].Contains("drainage"))
            {
                icons.container_drainage.SetActive(true);
            }
            else if (flower.atts["Containers"].Contains("large"))
            {
                icons.container_large.SetActive(true);
            }
            else if (flower.atts["Containers"].Contains("hanging"))
            {
                icons.container_hanging.SetActive(true);
            }
        }
        else
        {
            icons.container_potted.SetActive(true);
        }

        if (flower.atts.Keys.Contains("Fruit"))
        {
            if (flower.atts["Fruit"].ToLower().Contains("nut"))
            {
                icons.fruit_nut.SetActive(true);
            }
            else if (flower.atts["Fruit"].ToLower().Contains("berries"))
            {
                icons.fruit_berries.SetActive(true);
            }
            else if (flower.atts["Fruit"].ToLower().Contains("bean"))
            {
                icons.fruit_bean.SetActive(true);
            }
        }

        if (flower.atts.Keys.Contains("Flower Color"))
        {
            if (flower.atts["Flower Color"].ToLower().Contains("white"))
            {
                icons.color_white.SetActive(true);
            }
            else if (flower.atts["Flower Color"].ToLower().Contains("orange"))
            {
                icons.color_orange.SetActive(true);
            }
            else if (flower.atts["Flower Color"].ToLower().Contains("purple"))
            {
                icons.color_purple.SetActive(true);
            }
            else if (flower.atts["Flower Color"].ToLower().Contains("violet"))
            {
                icons.color_violet.SetActive(true);
            }
            else if (flower.atts["Flower Color"].ToLower().Contains("red"))
            {
                icons.color_red.SetActive(true);
            }
            else if (flower.atts["Flower Color"].ToLower().Contains("pink"))
            {
                icons.color_pink.SetActive(true);
            }
            else if (flower.atts["Flower Color"].ToLower().Contains("yellow"))
            {
                icons.color_yellow.SetActive(true);
            }
            else if (flower.atts["Flower Color"].ToLower().Contains("blue"))
            {
                icons.color_blue.SetActive(true);
            }
        }

        if (flower.atts.Keys.Contains("Toxicity"))
        {
            icons.tox.SetActive(true);
        }

        if (flower.details.ToLower().Contains("butterflies") || flower.display == "Debug")
            icons.butterflies.SetActive(true);

        // ToDo
        // flower.icons2 = icons;
        // flower.icons3 = icons;
        // flower.icons4 = icons;
        // flower.icons5 = icons;

        flower.rdy_icons = true;
        if (update)
            icons.all.SetActive(true);
    }

    public void saveFlowers(List<Flower> flowers_, string file_path_)
    {
        FlowerContainer container = new FlowerContainer(flowers_, _firstTime);
        string json = JsonUtility.ToJson(container);

        string path = file_path_ + "/Save.flowerpot";
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(path, FileMode.Create) {Position = 0};
        formatter.Serialize(stream, json);
        stream.Close();
    }

    private IEnumerator loadFlowers(string file_path_)
    {
        string path = file_path_ + "/Save.flowerpot";
        if (!File.Exists(path))
        {
            _flowers = new List<Flower>();
            loadingFinished();
            yield break;
        }

        BinaryFormatter formatter = new BinaryFormatter();
        FileStream stream = new FileStream(path, FileMode.Open) {Position = 0};
        string json = formatter.Deserialize(stream) as string;
        FlowerContainer container = JsonUtility.FromJson<FlowerContainer>(json);
        stream.Close();
        _flowers = container.flowerContainer;
        _firstTime = container.firstTime;
        _count.text = container.flowerContainer.Count.ToString();

        // yield return new WaitForEndOfFrame();

        foreach (Flower fl in _flowers)
        {
            // VuforiaARController.Instance.RegisterVuforiaStartedCallback(CreateImageTargetFromSideloadedTexture(fl));
            // StartCoroutine(CreateImageTargetFromSideloadedTexture(fl));

            if (fl.lastWatered == null || fl.lastWatered.Equals(string.Empty))
            {
                fl.lastWatered = (DateTime.Now - TimeSpan.FromDays(2)).ToString(CultureInfo.InvariantCulture);
                continue;
            }

            CreateMarker(fl, false);

            // if (fl.screen_path_2 != null && !fl.screen_path_2.Equals(string.Empty))
            //     addMarker(fl, fl.screen_path_2, fl.guid2);
            // if (fl.screen_path_3 != null && !fl.screen_path_3.Equals(string.Empty))
            //     addMarker(fl, fl.screen_path_3, fl.guid3);
            // if (fl.screen_path_4 != null && !fl.screen_path_4.Equals(string.Empty))
            //     addMarker(fl, fl.screen_path_4, fl.guid4);
            // if (fl.screen_path_5 != null && !fl.screen_path_5.Equals(string.Empty))
            //     addMarker(fl, fl.screen_path_5, fl.guid5);

            yield return new WaitForEndOfFrame();
        }

        updateView();
        // _text.text = "ready";
        // loadingFinished();
        StopCoroutine(loadFlowers(file_path_));
    }

    public void loadingFinished()
    {
        _bottom.SetActive(true);
        _welcome.SetActive(false);

        if (_firstTime)
        {
            _blueBack.SetActive(false);
            _firstTimeAdd = true;
            _firstTimeJournal = true;
        }
        else
        {
            _intro.SetActive(true);
            _firstTime = true;
            saveFlowers(_flowers, file_path);
        }
        
        _objectTracker.Start();
    }

    public void resetFirstTime()
    {
        _firstTime = false;
        saveFlowers(_flowers, file_path);
        _text.text = "Intro Reset";
    }

    private void setSprite(Flower flower, int i)
    {
        if (i < 7)
        {
            Image target = _backWork.transform.Find("Flower" + i).GetComponent<Image>();

            // set alpha
            Color c = target.color;
            c.a = 1;
            target.color = c;

            // load sprite from file
            Sprite sprite = LoadNewSprite(flower.small_path);
            target.overrideSprite = sprite;
        }
    }

    private Sprite LoadNewSprite(string FilePath, float PixelsPerUnit = 100.0f)
    {
        // Load a PNG or JPG image from disk to a Texture2D, assign this texture to a new sprite and return its reference

        Texture2D SpriteTexture = LoadTexture(FilePath);
        return Sprite.Create(SpriteTexture, new Rect(0, 0, SpriteTexture.width, SpriteTexture.height),
            new Vector2(0, 0), PixelsPerUnit);
    }

    private Texture2D LoadTexture(string FilePath)
    {
        // Load a PNG or JPG file from disk to a Texture2D
        // Returns null if load fails

        if (File.Exists(FilePath))
        {
            var FileData = File.ReadAllBytes(FilePath);
            var Tex2D = new Texture2D(2, 2);
            if (Tex2D.LoadImage(FileData)) // Load the imagedata into the texture (size is set automatically)
                return Tex2D; // If data = readable -> return texture
        }

        return null; // Return null if load failed
    }

    void deleteFlowers()
    {
        foreach (Flower ds in _flowers)
        {
            if (ds != null)
            {
                try
                {
                    _objectTracker.DeactivateDataSet(ds.dataset);
                }
                catch (Exception)
                {
                    // Debug.Log(e.StackTrace);
                }
            }
        }

        GameObject[] del = GameObject.FindGameObjectsWithTag("Target");

        foreach (GameObject target in del)
        {
            if (target != null)
            {
                target.gameObject.SetActive(false);
                // Destroy(target);
            }
        }

        _flowers = new List<Flower>();
        saveFlowers(_flowers, file_path);

        _count.text = "0";

        updateView();
        updatePageNavigation();

        // _text.text = "Deleted";
        // _debug.text = string.Empty;
    }

    void deleteOne(Flower flower)
    {
        _objectTracker.DeactivateDataSet(flower.dataset);
        _flowers.Remove(flower);
        _count.text = _flowers.Count.ToString();
        updateView();
        closeDetails();
        saveFlowers(_flowers, file_path);
        // StartCoroutine(loadFlowers(file_path));

        if (_flowers.Count % 6 == 0)
            _currentPage--;

        flower.trackable.SetActive(false);
    }

    [Serializable]
    public class Flower
    {
        public string guid;
        public string guid2;
        public string guid3;
        public string guid4;
        public string guid5;
        public string screen_path;
        public string screen_path_2;
        public string screen_path_3;
        public string screen_path_4;
        public string screen_path_5;
        public string small_path;

        public DataSet dataset;

        public Texture2D texSmall;

        public GameObject trackable;

        public float accuracy;
        public string display;
        public string latin;

        public Dictionary<string, string> atts;
        public string details;

        public Icons icons;
        public bool rdy_icons;
        public string lastWatered;

        public Flower()
        {
            atts = new Dictionary<string, string>();
        }
    }

    [Serializable]
    private class FlowerContainer
    {
        public List<Flower> flowerContainer;
        public bool firstTime;

        public FlowerContainer(List<Flower> flowers, bool first)
        {
            flowerContainer = flowers;
            firstTime = first;
        }
    }

    #endregion
}