using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
//using UnityEngine.Events;

public class PlaneHUD : MonoBehaviour
{
    public Rigidbody rb;
    public GameObject pPlane;
    public GameObject co;
    public GameObject goCanvas;
    public LineRenderer bombTrajectory;
    public Font hudFont;    //glass gauge
    public AudioSource lockingSFX;
    public AudioSource lockedSFX;
    public AudioSource missileSFX;
    public AudioSource missileProxSFX;
    public AudioSource stallSFX;
    //bool locked = false;

    private Rigidbody target;
    private Rigidbody nextTarget;
    GameObject[] ally;
    GameObject[] enemy;
    List<GameObject> enemyMissile;
    List<DroppedBomb> droppedBombs;

    public float scaleUI = 1f;
    //private Transform myTrans;
    private float widthHeightRatio;
    private float FoV = 60f;
    private float stallSpeed = 4.5f;
    private float pitchAngle;
    private float rollAngle;
    private float yawAngle;
    private float tSpeed;
    private float tAltitude;
    //float angleH = 5;
    private int numH = 38;
    private int numC = 130;
    private float angleH;
    private float angleC;
    private float ang2pixX;
    private float ang2pixY;
    private float compOffY = 0.4f;
    private float targetDistance;
    private float bulletSpeed = 100f;   //I should really grab these values from PlaneDriver but this works for now
    private float missileDist = 200f;
    private float gravity = 9.8f;
    private float lockSpeed = 0.5f;
    private float lockAngle = 45f;
    private float lockTimer = 0;
    private float fireDist = 75f;
    private int spike = 0;
    private float cannonFrac;
    int spMode = 1;

    private float cannonX;
    private float cannonY;

    //Quaternion angleOld;
    private Text textFire;
    private Text textSpeed;
    private Text textAltitude;
    private Text textStall;
    private Text textPSM;
    private GameObject AttIndicator;
    private Text textBearing;
    private Text textCannon;
    //private Text textPrograde;
    private Text textPointer;
    private Text textTarDist;
    private Text textTarLock;
    private Text textWarning;
    private Text textCannonAmmo;
    private GameObject[] textMissileCool;
    private GameObject[] fillSpCool;
    private Text textBomb;
    private Text flare;
    Vector3 bombPos;
    //private Vector3 backupBomb;
    //Text textAlly;
    //Text[] textHorizon;

    GameObject fire;
    GameObject stall;
    GameObject psm;
    GameObject[] tHorizon;
    GameObject[] compass;
    //List<GameObject> tEnemy;
    GameObject[] tAlly;     //Displays box around allies
    GameObject[] tEnemy;    //Displays box around enemies
    GameObject bearing;     //Displays compass bearing
    GameObject cannon;      //Cannon reticle accounts for deflection
    GameObject prograde;    //Points to prograde vector
    GameObject pointer;     //Points to target when it is out of sight
    GameObject tarDist;     //Displays target distance
    GameObject tarLock;
    GameObject warning;
    GameObject tNextTarget;
    GameObject cannonAmmoMask;
    List<GameObject> missilePointer;
    List<GameObject> bombMarker;
    GameObject[] missileLoad;
    GameObject[] spLoad;
    
    GameObject bomb;

    GameObject minimap;
    GameObject[] mapEnemy;
    GameObject[] mapAlly;
    GameObject[] wingmanStatus;
    

    private string line = '\u2500'.ToString();
    private string lineV = '\u2502'.ToString();
    private string dot = '\u2504'.ToString();
    private string box = '\u2610'.ToString();
    private string boxCross = '\u2612'.ToString();
    private string boxDiamond = '\u26CB'.ToString();
    private string diamond = '\u25C7'.ToString();
    private string boxCorner = '\u26F6'.ToString();
    //private string gear = '\u263C'.ToString();
    //private string sun = '\u2609'.ToString();
    //private string sun = "⚙" + "\n" + "·";
    private string diagCross = '\u2573'.ToString();
    private string pointerArrow = '\u25B7'.ToString();
    private string solidArrow = '\u25BA'.ToString();
    //string squareCross = '\u2BD0'.ToString();
    //string corners = '\u26F6'.ToString();
    private string openCross = '\u2613'.ToString();
    private string tTrail = '\u2577'.ToString() +"\n"+ '\u2576'.ToString() + '\u25CB'.ToString()+ '\u2574'.ToString() + "\n ";
    //string line = '\u25B2'.ToString();
    private string circle = '\u25CB'.ToString();
    //char upArrow = '\u25B2';
    string horizonTop;
    string horizonBot;

    private bool missileProx = false;
    //Color color1;   //green old colors im not really using
    //Color color2;   //blue
    //Color color3;   //red
    //Text[] textHorizon2;

    private void OnEnable()
    {
        //Subscribe to events
        //be sure to leave a like and click the bell ladsfasdfawe
        EnemyHealth.OnDeath += Recount;
        MissileTrack.MissilePlayer += AddWarning;
        MissileTrack.MissileLost += SubWarning;
        BogeyAI.radarSpike += CountSpikes;
        SAMsiteAI.radarSpike += CountSpikes;
        PlaneDriver.targSwitch += UpdateTarget;
        PlaneDriver.targNext += UpdateNextTarget;
        PlaneDriver.spSwitch += UpdateSP;
        PlaneDriver.dropBomb += AddBomb;
        BombTrigger.bombExplode += SubBomb;
    }

    private void OnDisable()
    {
        EnemyHealth.OnDeath -= Recount;
        MissileTrack.MissilePlayer -= AddWarning;
        MissileTrack.MissileLost -= SubWarning;
        BogeyAI.radarSpike -= CountSpikes;
        SAMsiteAI.radarSpike -= CountSpikes;
        PlaneDriver.targSwitch -= UpdateTarget;
        PlaneDriver.targNext -= UpdateNextTarget;
        PlaneDriver.spSwitch -= UpdateSP;
        PlaneDriver.dropBomb -= AddBomb;
        BombTrigger.bombExplode -= SubBomb;
    }

    // Start is called before the first frame update
    void Start()
    {
        enemyMissile = new List<GameObject>();
        missilePointer = new List<GameObject>();
        droppedBombs = new List<DroppedBomb>();
        //angleOld = rb.transform.rotation;
        //targetDistance = 100f;
        //Transform child = goCanvas.transform.Find("Speed");
        lockSpeed = rb.GetComponent<PlaneDriver>().lockSpeed;
        lockAngle = rb.GetComponent<PlaneDriver>().lockAngle;

        string lineSolid = line + line + line;
        string lineDotted = dot + dot + dot;
        horizonTop = lineSolid + "                   " + lineSolid;
        horizonBot = lineDotted + "                   " + lineDotted;

        //ColorUtility.TryParseHtmlString("#00E514", out color1);
        //ColorUtility.TryParseHtmlString("#00E0FF", out color2);
        //ColorUtility.TryParseHtmlString("#FF0000", out color3);

        missileLoad = new GameObject[2];    //< hard coding the size probably isnt good
        textMissileCool = new GameObject[missileLoad.Length];
        spLoad = new GameObject[2];
        fillSpCool = new GameObject[spLoad.Length];
        //missileFrac = new float[missileLoad.Length];

        fire = goCanvas.transform.Find("Fire").gameObject;
        stall = goCanvas.transform.Find("Stall").gameObject;
        psm = goCanvas.transform.Find("PSM").gameObject;
        warning = goCanvas.transform.Find("Warning").gameObject;
        cannonAmmoMask = goCanvas.transform.Find("CannonAmmoMask").gameObject;
        cannon = goCanvas.transform.Find("Cannon").gameObject;
        missileLoad[0] = goCanvas.transform.Find("MissileLoad1").gameObject;
        missileLoad[1] = goCanvas.transform.Find("MissileLoad2").gameObject;
        prograde = goCanvas.transform.Find("Prograde").gameObject;
        tNextTarget = goCanvas.transform.Find("NextTarget").gameObject;

        spLoad[0] = goCanvas.transform.Find("BombLoad1").gameObject;
        spLoad[1] = goCanvas.transform.Find("BombLoad2").gameObject;

        textFire = fire.GetComponent<Text>();
        textSpeed = goCanvas.transform.Find("Speed").GetComponent<Text>();
        textAltitude = goCanvas.transform.Find("Altitude").GetComponent<Text>();
        textStall = stall.GetComponent<Text>();
        textPSM = psm.GetComponent<Text>();
        AttIndicator = goCanvas.transform.Find("Cursor").gameObject;
        textWarning = warning.GetComponent<Text>();
        textCannonAmmo = cannonAmmoMask.transform.Find("CannonAmmo").gameObject.GetComponent<Text>();
        textCannon = cannon.GetComponent<Text>();
        flare = goCanvas.transform.Find("Flare").gameObject.GetComponent<Text>();

        for (int i=0; i < missileLoad.Length; i++)
        {
            textMissileCool[i] = missileLoad[i].transform.Find("MissileLoadMask").transform.Find("CoolDown").gameObject;
        }

        for (int i = 0; i < spLoad.Length; i++)
        {
            fillSpCool[i] = spLoad[i].transform.Find("BombLoadMask").transform.Find("CoolDown").gameObject;
        }

        minimap = goCanvas.transform.Find("Minimap").gameObject;

        //Adjust UI placement based on screen size
        textFire.transform.position = new Vector3(Screen.width / 2f, Screen.height * 0.3f, 0);
        textSpeed.transform.position = new Vector3(Screen.width*0.2f, Screen.height/2f, 0);
        textAltitude.transform.position = new Vector3(Screen.width * 0.8f, Screen.height / 2f, 0);
        textStall.transform.position = new Vector3(Screen.width /2f, Screen.height * 0.25f, 0);
        textPSM.transform.position = new Vector3(Screen.width / 2f, Screen.height * 0.1f, 0);
        missileLoad[0].transform.position = new Vector3(Screen.width * 0.8f, Screen.height * 0.2f, 0);
        missileLoad[1].transform.position = new Vector3(Screen.width * 0.84f, Screen.height * 0.2f, 0);
        spLoad[0].transform.position = new Vector3(Screen.width * 0.8f, Screen.height * 0.2f, 0);
        spLoad[1].transform.position = new Vector3(Screen.width * 0.84f, Screen.height * 0.2f, 0);
        flare.transform.position = new Vector3(Screen.width * 0.82f, Screen.height * 0.07f, 0);
        minimap.transform.position = new Vector3(Screen.width * 0.1f, Screen.height * 0.2f, 0);

        //Got a lot of UI objects to instantiate
        bearing = new GameObject("Bearing");
        bearing.transform.SetParent(goCanvas.transform);
        textBearing = bearing.AddComponent<Text>();
        textBearing.transform.position = new Vector3(Screen.width/2f, 0.8f * Screen.height, 0);
        textBearing.font = hudFont;
        textBearing.fontSize = 40;
        textBearing.color = Color.green;
        textBearing.alignment = TextAnchor.MiddleCenter;
        textBearing.rectTransform.sizeDelta = new Vector2(60, 60);

        pointer = new GameObject("Pointer");
        pointer.transform.SetParent(goCanvas.transform);
        textPointer = pointer.AddComponent<Text>();
        textPointer.transform.position = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
        textPointer.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
        textPointer.fontSize = 140;
        textPointer.color = Color.green;
        textPointer.alignment = TextAnchor.MiddleCenter;
        textPointer.rectTransform.sizeDelta = new Vector2(60, 60);
        textPointer.text = pointerArrow;

        tarDist = new GameObject("TargetDistance");
        tarDist.transform.SetParent(goCanvas.transform);
        textTarDist = tarDist.AddComponent<Text>();
        textTarDist.transform.position = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
        textTarDist.font = hudFont;
        textTarDist.fontSize = 35;
        textTarDist.color = Color.green;
        textTarDist.alignment = TextAnchor.LowerCenter;
        //textTarDist.text = pointerArrow;

        tarLock = new GameObject("TargetLock");
        tarLock.transform.SetParent(goCanvas.transform);
        textTarLock = tarLock.AddComponent<Text>();
        textTarLock.transform.position = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
        textTarLock.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
        textTarLock.fontSize = 140;
        textTarLock.color = Color.green;
        textTarLock.alignment = TextAnchor.MiddleCenter;
        textTarLock.text = diamond;
        tarLock.SetActive(false);

        bomb = new GameObject("Bomb");
        bomb.transform.SetParent(goCanvas.transform);
        textBomb = bomb.AddComponent<Text>();
        textBomb.transform.position = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
        textBomb.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
        textBomb.fontSize = 300;
        textBomb.color = Color.green;
        textBomb.alignment = TextAnchor.MiddleCenter;
        textBomb.text = circle;
        bomb.SetActive(false);

        tHorizon = new GameObject[numH - 1];
        compass = new GameObject[numC - 1];
        
        CountAllies();
        CountEnemies();

        angleH = 190f / (float)numH;
        angleC = 390f / (float)numC;
        ang2pixX = Screen.width / FoV;
        ang2pixY = Screen.height / FoV;

        //Initialize horizon
        for (int i=0;i<tHorizon.Length;i++)
        {
            tHorizon[i] = new GameObject("HorizonTest");
            tHorizon[i].transform.SetParent(goCanvas.transform);
            Text currText = tHorizon[i].AddComponent<Text>();
            string tAngle = ((int)(angleH * (i + 1 - numH / 2))).ToString();
            if (angleH * (i + 1 - numH / 2) >= 0)
            {
                currText.text = tAngle + horizonTop + tAngle;
            }
            else
            {
                currText.text = tAngle + horizonBot + tAngle;
            }
                
            currText.transform.position = new Vector3(50f, i*50f, 0);
            //currText.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            currText.font = hudFont;
            currText.fontSize = 40;
            currText.color = Color.green;
            currText.alignment = TextAnchor.MiddleCenter;
            currText.rectTransform.sizeDelta = new Vector2(1000, 600);
            //currText.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTranform.Axis.Vertical, myHeight);
        }

        //Initialize compass
        for (int i = 0; i < compass.Length; i++)
        {
            compass[i] = new GameObject("Compass");
            compass[i].transform.SetParent(goCanvas.transform);
            Text currText = compass[i].AddComponent<Text>();
            currText.transform.position = new Vector3(Screen.width*0.1f*i, 0.8f*Screen.height, 0);
            currText.font = hudFont;
            currText.fontSize = 40;
            currText.color = Color.green;
            currText.alignment = TextAnchor.LowerCenter;
            currText.rectTransform.sizeDelta = new Vector2(60, 60);
            int az = (i+54) * (int)angleC % 360;
            if (az%360==0)
            {
                currText.text = "N\n" + lineV;
            }
            else if (az % 360 == 45)
            {
                currText.text = "NE\n" + lineV;
            }
            else if (az % 360 == 90)
            {
                currText.text = "E\n" + lineV;
            }
            else if (az % 360 == 135)
            {
                currText.text = "SE\n" + lineV;
            }
            else if (az % 360 == 180)
            {
                currText.text = "S\n" + lineV;
            }
            else if (az % 360 == 225)
            {
                currText.text = "SW\n" + lineV;
            }
            else if (az % 360 == 270)
            {
                currText.text = "W\n" + lineV;
            }
            else if (az % 360 == 315)
            {
                currText.text = "NW\n" + lineV;
            }
            else
            {
                currText.text = lineV;
            }

            //currText.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTranform.Axis.Vertical, myHeight);
            
        }

        wingmanStatus = new GameObject[pPlane.GetComponent<PlaneDriver>().wingman.Length];
        for (int i = 0; i < pPlane.GetComponent<PlaneDriver>().wingman.Length; i++)
        {
            wingmanStatus[i] = new GameObject("WingmanStatus");
            wingmanStatus[i].transform.SetParent(goCanvas.transform);
            Text textWmStatus = wingmanStatus[i].AddComponent<Text>();
            textWmStatus.transform.position = new Vector3(Screen.width * 0.21f, Screen.height * (0.25f - 0.05f*i), 0);
            textWmStatus.font = hudFont;
            textWmStatus.fontSize = 35;
            textWmStatus.color = Color.green;
            textWmStatus.alignment = TextAnchor.MiddleLeft;
            textWmStatus.text = "WM" + (i+1).ToString() + ":FOLLOW";
            SetTextFlow(textWmStatus);
            //textWmStatus.SetActive(false);
        }

        //Change ui scale depending on screen size
        foreach (Transform child in goCanvas.transform)
        {
            
            if (child.GetComponent<Text>() != null)
            {
                SetScale(child.transform, scaleUI, 1f);
                Text currText = child.GetComponent<Text>();
                SetTextFlow(currText);
                //print(currText);
            }
            if (child.GetComponent<Image>() != null)
            {
                SetScale(child.transform, scaleUI, 1f);
            }
        }
        //Wanna have this always rendered in front
        warning.transform.SetAsLastSibling();
    }


    //Late Update is called once AFTER each frame
    //Update also works but sometimes jitters
    //Update works now switched camera to FixedUpdate
    void Update()
    {

        float flareReady = pPlane.GetComponent<PlaneDriver>().flareCool - (Time.time - pPlane.GetComponent<PlaneDriver>().flareTime);
        if (flareReady < 0)
        {
            flare.text = "[FLR] RDY";
        }
        else
        {
            flare.text = "[FLR] " + flareReady.ToString("F1");
        }


        for (int i = 0; i < pPlane.GetComponent<PlaneDriver>().wingman.Length; i++)
        {
            Text textWmStatus = wingmanStatus[i].GetComponent<Text>();
            int wingmanMode = pPlane.GetComponent<PlaneDriver>().wingman[i].GetComponent<WingmanAI>().mode;
            if (wingmanMode == 1)
            {
                textWmStatus.text = "WM" + (i + 1).ToString() + ":FOLLOW";
            }
            else if (wingmanMode == 2)
            {
                textWmStatus.text = "WM" + (i + 1).ToString() + ":ATTACK";
            }
        }

        if (enemyMissile.Count > 0)
        {
            missileProx = false;
            foreach (GameObject missile in enemyMissile)
            {
                float missileWarningDist = Vector3.Distance(rb.position, missile.transform.position);
                if (missileWarningDist < 40f)
                {
                    missileProx = true;
                }
            }

            if (!missileSFX.isPlaying && !missileProx)
            {
                missileSFX.Play();
                missileProxSFX.Stop();
            }
            else if (!missileProxSFX.isPlaying && missileProx)
            {
                missileSFX.Stop();
                missileProxSFX.Play();
            }
        }
        else
        {
            missileSFX.Stop();
            missileProxSFX.Stop();
        }
            

        //target = rb.target;
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            lockTimer = Time.time + lockSpeed;
        }

        //target = rb.GetComponent<PlaneDriver>().target;
        tSpeed = rb.velocity.magnitude;
        tAltitude = pPlane.transform.position.y;

        if (target == null)
        {
            fire.SetActive(false);
        }

        //Calculate RPY values of plane
        rollAngle = rb.transform.rotation.eulerAngles.z;
        pitchAngle = -RelAngle(rb.transform.rotation.eulerAngles.x);
        yawAngle = -RelAngle(rb.transform.rotation.eulerAngles.y);

        //Calculate RPY values of camera
        float cameraPitch = -RelAngle(co.transform.localEulerAngles.x);
        float cameraRoll = co.transform.rotation.eulerAngles.z;
        float cameraYaw = -RelAngle(co.transform.localEulerAngles.y);

        //Cursor pitch and yaw stuff
        float cursorPit = -((cameraPitch) * ang2pixY);
        float cursorYaw = -((cameraYaw) * ang2pixY);

        float cursorY = cursorPit + (Screen.height / 2f);
        float cursorX = cursorPit * Mathf.Sin(Mathf.PI * cameraRoll / 180f) + Screen.width / 2f;
        cursorX = Screen.width / 2f - cursorYaw;

        float horizonY = cursorY + -pitchAngle * ang2pixY * Mathf.Cos(cameraRoll * Mathf.Deg2Rad);
        float horizonX = cursorX + -pitchAngle * ang2pixY * Mathf.Sin(cameraRoll * Mathf.Deg2Rad);

        float compassX = cursorX + yawAngle * ang2pixX * Mathf.Cos(cameraRoll * Mathf.Deg2Rad);
        float compassY = cursorY - yawAngle * ang2pixX * Mathf.Sin(cameraRoll * Mathf.Deg2Rad);
        compassX = compassX + compOffY * Screen.height * Mathf.Sin(cameraRoll * Mathf.Deg2Rad);
        compassY = compassY + compOffY * Screen.height * Mathf.Cos(cameraRoll * Mathf.Deg2Rad);

        float bearingX = cursorX + 0.3f * Screen.height * Mathf.Sin(Mathf.PI * cameraRoll / 180f);
        float bearingY = cursorY + 0.3f * Screen.height * Mathf.Cos(Mathf.PI * cameraRoll / 180f);


        Vector3 relAngleVel = rb.transform.InverseTransformDirection(rb.angularVelocity).normalized
                * rb.angularVelocity.magnitude;

        float angleX = relAngleVel.y * Mathf.Rad2Deg * ang2pixY;// 
        float angleY = relAngleVel.x * Mathf.Rad2Deg * ang2pixY;

        if (target != null)
        {
            targetDistance = Vector3.Distance(target.transform.position, rb.transform.position);
            float dt = targetDistance / (bulletSpeed+tSpeed);
            float drop = 0.5f * gravity * Mathf.Pow(dt, 2);
            float dropAngle = Mathf.Atan2(drop * Mathf.Cos(pitchAngle * Mathf.Deg2Rad),
                (targetDistance - drop * Mathf.Sin(pitchAngle * Mathf.Deg2Rad))) * Mathf.Rad2Deg;

            //print(targetDistance);

            cannonX = cursorX - dropAngle * ang2pixY * Mathf.Sin(cameraRoll * Mathf.Deg2Rad);
            cannonY = cursorY - dropAngle * ang2pixY * Mathf.Cos(cameraRoll * Mathf.Deg2Rad);
            cannonX += -angleX*dt* Mathf.Cos(relAngleVel.z*dt/2f) + angleY * dt * Mathf.Sin(relAngleVel.z * dt/2f);
            cannonY += angleY * dt * Mathf.Cos(relAngleVel.z * dt/2f) + angleX * dt * Mathf.Sin(relAngleVel.z * dt/2f);

            cannonFrac = (float)rb.GetComponent<PlaneDriver>().cannonCurr / (float)rb.GetComponent<PlaneDriver>().cannonMax;

            textCannon.transform.position = new Vector3(cannonX, cannonY, 0);
            cannonAmmoMask.transform.position = new Vector3(cannonX, cannonY, 0);
            textCannonAmmo.transform.eulerAngles = new Vector3(0, 0, -180 * cannonFrac);

            cannon.SetActive(targetDistance < fireDist);
            cannonAmmoMask.SetActive(targetDistance < fireDist);


            //Text textPointer = pointer.GetComponent<Text>();
            Vector3 posTarget = Camera.main.WorldToScreenPoint(target.transform.position);
            string targetString = target.gameObject.GetComponent<NameTag>().nametype;
            targetString += "\n" + ((int)(targetDistance * 10)).ToString();
            textTarDist.text = targetString;

            float posMag = new Vector2(posTarget.x - Screen.width/2f, posTarget.y - Screen.height / 2f).magnitude;

            if ((posMag > 0.45f * Screen.height) || (posTarget.z < 0))
            {
                //Vector3 targetRelPos = co.transform.InverseTransformVector(target.transform.position - rb.transform.position);
                pointer.SetActive(true);
                posTarget -= new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
                posTarget.x *= posTarget.z;
                posTarget.y *= posTarget.z;
                float angleTarget = Mathf.Atan2(posTarget.y, posTarget.x) * Mathf.Rad2Deg;

                //float targetRelAng = posTarget.magnitude;
                posTarget = posTarget.normalized * 0.45f * Screen.height;
                //print((-posTarget.z + 2f) / 4f);
                if ((-posTarget.z + 4f) != 0)
                {
                    SetScale(pointer.transform, 1f, 4f / (-posTarget.z + 4f));
                }
                

                posTarget += new Vector3(Screen.width / 2f, Screen.height / 2f, 0);

                textPointer.transform.eulerAngles = new Vector3(0, 0, angleTarget);
                textPointer.transform.position = posTarget;
            }
            else
            {
                pointer.SetActive(false);
            }
            
        }
        else
        {
            pointer.SetActive(false);
            tarDist.SetActive(false);
            cannon.SetActive(false);
            cannonAmmoMask.SetActive(false);
            tarLock.SetActive(false);
        }

        if (missilePointer != null)
        {
            for(int i=0; i < missilePointer.Count; i++)
            {
                //print("test");
                Text textMissPointer = missilePointer[i].GetComponent<Text>();
                Vector3 posTarget = Camera.main.WorldToScreenPoint(enemyMissile[i].transform.position);

                float posMag = new Vector2(posTarget.x - Screen.width / 2f, posTarget.y - Screen.height / 2f).magnitude;
                

                if ((posMag > 0.45f * Screen.height) || (posTarget.z < 0))
                {
                    //Vector3 targetRelPos = co.transform.InverseTransformVector(target.transform.position - rb.transform.position);
                    missilePointer[i].SetActive(true);
                    posTarget -= new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
                    posTarget.x *= posTarget.z;
                    posTarget.y *= posTarget.z;
                    float angleTarget = Mathf.Atan2(posTarget.y, posTarget.x) * Mathf.Rad2Deg;

                    //float targetRelAng = posTarget.magnitude;
                    posTarget = posTarget.normalized * 0.45f * Screen.height;
                    //print((-posTarget.z + 2f) / 4f);
                    if ((-posTarget.z + 4f) != 0)
                    {
                        SetScale(missilePointer[i].transform, 1f, 4f / (-posTarget.z + 4f));
                    }


                    posTarget += new Vector3(Screen.width / 2f, Screen.height / 2f, 0);

                    textMissPointer.transform.eulerAngles = new Vector3(0, 0, angleTarget);
                    textMissPointer.transform.position = posTarget;
                }
                else
                {
                    missilePointer[i].SetActive(false);
                }
            }
        }
        else
        {
            //Turn off missile warning
        }

        //textCursor.transform.eulerAngles = new Vector3(0, 0, -cameraRoll);

        Vector3 progradeDir = rb.transform.InverseTransformDirection(rb.velocity.normalized);
        //print(progradeDir);

        float progX = ang2pixY * Mathf.Rad2Deg * (Mathf.Atan2(progradeDir.x, progradeDir.z));
        float progY = ang2pixY * Mathf.Rad2Deg * (Mathf.Atan2(progradeDir.y, progradeDir.z));
        prograde.transform.position = new Vector3(cursorX + progX, cursorY + progY, 0);
        AttIndicator.transform.position = new Vector3(cursorX, cursorY, 0);
        
        //update horizon
        for (int i = 0; i < tHorizon.Length; i++)
        {
            Text currText = tHorizon[i].GetComponent<Text>();
            //currText.text = horizonTop;
            currText.transform.eulerAngles = new Vector3(0, 0, -cameraRoll);
            float currS = angleH * (i + 1 - numH / 2);
            float currX = horizonX + currS * ang2pixY * Mathf.Sin(Mathf.Deg2Rad * cameraRoll);
            float currY = horizonY + currS * ang2pixY * Mathf.Cos(Mathf.Deg2Rad * cameraRoll);
            currText.transform.position = new Vector3(currX, currY, 0);

            tHorizon[i].SetActive(Mathf.Abs(currS - (pitchAngle)) < 18f);
        }

        //update compass
        for (int i = 0; i < compass.Length; i++)
        {
            Text currText = compass[i].GetComponent<Text>();
            //currText.text = horizonTop;
            currText.transform.eulerAngles = new Vector3(0, 0, -cameraRoll);
            float currS = (angleC * (i - 1 - numC / 2));
            //float currS = (angleC * (i-1)) % 360 - 180;
            float currX = compassX + currS * ang2pixX * Mathf.Cos(Mathf.Deg2Rad * cameraRoll);
            float currY = compassY - currS * ang2pixX * Mathf.Sin(Mathf.Deg2Rad * cameraRoll);
            currText.transform.position = new Vector3(currX, currY, 0);

            compass[i].SetActive(Mathf.Abs(currS + (yawAngle)) < 12f);
        }

        //Rotate minimap
        minimap.transform.Find("MapElements").gameObject.transform.localEulerAngles =
                new Vector3(0f, 0f, rb.transform.rotation.eulerAngles.y);

        //update ally markers
        for (int i=0;i<ally.Length;i++)
        {
            Vector3 posAlly = Camera.main.WorldToScreenPoint(ally[i].transform.position);
            Text textAlly = tAlly[i].GetComponent<Text>();
            textAlly.transform.position = posAlly;
            tAlly[i].SetActive(posAlly.z > 0);

            Text textMapAlly = mapAlly[i].GetComponent<Text>();
            Vector3 mapAllyPos = (ally[i].transform.position - rb.transform.position) * Screen.width / 1000f;
            textMapAlly.transform.localPosition = new Vector3(mapAllyPos.x, mapAllyPos.z, 0f);
            textMapAlly.transform.localEulerAngles = new Vector3(0f, 0f, -ally[i].transform.rotation.eulerAngles.y);
            //textMapAlly.transform.eulerAngles = new Vector3(0f, 0f, ally[i].transform.rotation.eulerAngles.y);
            //print(posAlly);
        }

        //update enemy markers
        for (int i = 0; i < tEnemy.Length; i++)
        {
            Vector3 posEnemy = Camera.main.WorldToScreenPoint(enemy[i].transform.position);
            Text textEnemy = tEnemy[i].GetComponent<Text>();
            textEnemy.transform.position = posEnemy;
            tEnemy[i].SetActive(posEnemy.z > 0);
            textEnemy.text = box;

            Text textMapEnemy = mapEnemy[i].GetComponent<Text>();
            Vector3 mapEnemyPos = (enemy[i].transform.position - rb.transform.position) * Screen.width / 1000f;
            textMapEnemy.transform.localPosition = new Vector3(mapEnemyPos.x, mapEnemyPos.z, 0f);
            textMapEnemy.transform.localEulerAngles = new Vector3(0f, 0f, -enemy[i].transform.rotation.eulerAngles.y);
            
            for (int j = 0; j < pPlane.GetComponent<PlaneDriver>().wingman.Length; j++)
            {
                GameObject currentWingman = pPlane.GetComponent<PlaneDriver>().wingman[j];
                Rigidbody currentTarget = currentWingman.GetComponent<WingmanAI>().target;
                if (enemy[i].GetComponent<Rigidbody>() == currentTarget && currentWingman.GetComponent<WingmanAI>().mode == 2)
                {
                    //textEnemy.text = box + "\n▻ ◅";
                    textEnemy.text = "☐\nˇ";
                }
            }

            if (enemy[i].GetComponent<Rigidbody>() == nextTarget)
            {
                Vector3 tarDistOff = new Vector3(0, -0.06f * Screen.height, 0);
                tNextTarget.transform.position = posEnemy + tarDistOff;
                tNextTarget.SetActive(posEnemy.z > 0);
            }

            //target locking
            //might want to make this go in planeDriver script and use FixedUpdate but it works for now
            if ((enemy[i].GetComponent<Rigidbody>() == target) && (posEnemy.z > 0))
            {
                Vector2 pos2d = new Vector2(posEnemy.x - cannonX, posEnemy.y - cannonY);
                float cannonDist = pos2d.magnitude / ang2pixY;

                tarDist.SetActive(posEnemy.z > 0);
                Vector3 tarDistOff = new Vector3(0, -0.06f * Screen.height, 0);
                textTarDist.transform.position = posEnemy + tarDistOff;

                if ((cannonDist < 5f) && (targetDistance < fireDist))
                {
                    fire.SetActive(true);
                }
                else
                {
                    fire.SetActive(false);
                }

                //When firing normal missiles
                if (spMode == 1)
                {
                    //bombTrajectory.gameObject.SetActive(false);
                    Vector2 lock2d = new Vector2(posEnemy.x - cursorX, posEnemy.y - cursorY);
                    float currLockAng = lock2d.magnitude / ang2pixX;

                    //If target is within missile scope
                    tarLock.SetActive((currLockAng < lockAngle) && (posEnemy.z > 0) && (targetDistance < missileDist));
                    if ((currLockAng < lockAngle) && (lockTimer != 0f) && (targetDistance < missileDist))
                    {
                        if (!lockingSFX.isPlaying && !lockedSFX.isPlaying)
                        {
                            lockingSFX.Play();
                        }

                        //Needs time to lock on
                        if (lockTimer < Time.time)
                        {
                            lockingSFX.Stop();
                            if (!lockedSFX.isPlaying)
                            {
                                lockedSFX.Play();
                            }

                            textEnemy.color = Color.red;
                            textTarLock.transform.position = posEnemy;
                            textTarLock.color = Color.red;
                            rb.GetComponent<PlaneDriver>().locked = true;
                            //locked = true;
                            //print("LOCKED");
                        }
                        else
                        {
                            lockedSFX.Stop();
                            if (!lockingSFX.isPlaying)
                            {
                                lockingSFX.Play();
                            }

                            float lockFactor = (1f - (lockTimer - Time.time) / lockSpeed);
                            lock2d *= lockFactor;
                            textTarLock.transform.position = new Vector3(cursorX + lock2d.x, cursorY + lock2d.y, 0);
                            textTarLock.color = Color.green;
                            rb.GetComponent<PlaneDriver>().locked = false;
                            //locked = false;
                            //print("locking");
                        }
                    }
                    else
                    {
                        lockingSFX.Stop();
                        lockedSFX.Stop();

                        lockTimer = Time.time + lockSpeed;
                        textEnemy.color = Color.green;
                        textTarLock.color = Color.green;
                        rb.GetComponent<PlaneDriver>().locked = false;
                        //locked = false;
                        //print("no lock");
                    }

                    //if (droppedBombs != null)
                }
                else
                {
                    lockingSFX.Stop();
                    lockedSFX.Stop();
                    textEnemy.color = Color.green;
                }
                //textEnemy.color = Color.red;
            }
            else
            {
                textEnemy.color = Color.green;
            }
        }

        if (spMode == 1)
        {
            for (int i = 0; i < missileLoad.Length; i++)
            {
                float missileFrac = (Time.time - rb.GetComponent<PlaneDriver>().missileTime[i]) / rb.GetComponent<PlaneDriver>().missileCool;
                missileFrac = Mathf.Clamp(missileFrac, 0f, 1f);
                textMissileCool[i].transform.localScale = new Vector3(1, missileFrac, 1);
            }
        }
        else if (spMode == 2)
        {
            for (int i = 0; i < spLoad.Length; i++)
            {
                float spFrac = (Time.time - rb.GetComponent<PlaneDriver>().spTime[i]) / rb.GetComponent<PlaneDriver>().bombCool;
                spFrac = Mathf.Clamp(spFrac, 0f, 1f);
                fillSpCool[i].transform.localScale = new Vector3(1, spFrac, 1);
            }
        }
        

        //If dropping bombs
        if (spMode == 2)
        {
            bombTrajectory.gameObject.SetActive(true);
            bomb.SetActive(true);
            lockTimer = Time.time + lockSpeed;
            textTarLock.color = Color.green;
            rb.GetComponent<PlaneDriver>().locked = false;
            tarLock.SetActive(false);


            
            RaycastHit bombTarget = GravCast(rb.transform.position, rb.velocity, 120);
            //RaycastHit bombTarget = GravCast(rb.transform.position, rb.transform.forward*100f, 180);
            if (bombTarget.collider != null)
            {
                bombPos = bombTarget.point;

                Vector3 relBombPos = Camera.main.transform.InverseTransformPoint(bombPos);
                Vector3 bombCamera = Camera.main.WorldToScreenPoint(bombPos);

                if (bombCamera.z > 0)
                {
                    textBomb.transform.position = bombCamera;
                    textBomb.transform.eulerAngles = new Vector3(0, 0, -cameraRoll);

                    if (relBombPos.sqrMagnitude != 0)
                    {
                        //print(cameraPitch + pitchAngle);
                        //float dropAngle = Vector3.Angle(relBombPos, Vector3.forward);
                        float dropAngle = Vector3.Angle(rb.transform.InverseTransformPoint(bombPos), Vector3.forward);
                        
                        float bombRatio = Mathf.Abs(Mathf.Sin((pitchAngle - dropAngle) * Mathf.Deg2Rad));
                        Vector3 toBomb = bombPos - Camera.main.transform.position;
                        //Quaternion dropAngle = Quaternion.LookRotation(Camera.main.transform.InverseTransformDirection(toBomb), Vector3.up);
                        Vector3 tiltDir = Quaternion.FromToRotation(Camera.main.transform.InverseTransformDirection(Camera.main.transform.forward),
                            Camera.main.transform.InverseTransformDirection(Vector3.up)).eulerAngles;
                        //dropAngle = Camera.main.transform.InverseTransformDirection
                        //dropAngle *= Quaternion.Inverse(Camera.main.transform.rotation);
                        //print(dropAngle);
                        /*
                        Vector3 dropAngle = Quaternion.FromToRotation(Camera.main.transform.InverseTransformDirection(Camera.main.transform.forward),
                            Camera.main.transform.InverseTransformDirection(toBomb)).eulerAngles;
                            */
                        //Vector3 tiltDir = Vector3.Cross(Vector3.right, toBomb);
                        Quaternion tiltRot = new Quaternion();
                        tiltRot.SetLookRotation(tiltDir, Camera.main.transform.up);
                        //tiltRot.SetLookRotation(Camera.main.transform.InverseTransformDirection(toBomb));
                        //tiltRot = tiltRot * Quaternion.Inverse(dropAngle);
                        //print(tiltDir);
                        SetScale(bomb.transform, 200f / relBombPos.magnitude, bombRatio);
                        //SetScale(bomb.transform, 200f / relBombPos.magnitude, 1);
                        //textBomb.transform.rotation = dropAngle;
                        //textBomb.transform.eulerAngles = new Vector3(dropAngle.x + tiltDir.x, dropAngle.y + tiltDir.y, tiltDir.z);
                        /*
                        textBomb.transform.eulerAngles = new Vector3(
                            (pitchAngle - dropAngle) * Mathf.Cos(cameraRoll*Mathf.Deg2Rad),
                            (pitchAngle - dropAngle) * Mathf.Sin(cameraRoll * Mathf.Deg2Rad), -cameraRoll);
                            */
                    }
                    //print(relBombPos.magnitude);
                }
                else
                {
                    bomb.SetActive(false);
                }
            }
            //print(bombPos);
            else
            {
                bomb.SetActive(false);
                bombTrajectory.gameObject.SetActive(false);
            }

            
        }
        else
        {
            bomb.SetActive(false);
            bombTrajectory.gameObject.SetActive(false);
        }

        if (droppedBombs != null)
        {
            foreach (DroppedBomb curBomb in droppedBombs)
            {

                Vector3 relBombPos = Camera.main.transform.InverseTransformPoint(curBomb.BombPosition);
                Vector3 bombCamera = Camera.main.WorldToScreenPoint(curBomb.BombPosition);
                Text textMarker = curBomb.Marker.GetComponent<Text>();
                if (bombCamera.z > 0)
                {
                    curBomb.Marker.SetActive(true);
                    textMarker.transform.position = bombCamera;
                    textMarker.transform.eulerAngles = new Vector3(0, 0, -cameraRoll);

                    if (relBombPos.sqrMagnitude != 0)
                    {
                        //print(cameraPitch + pitchAngle);
                        //float dropAngle = Vector3.Angle(relBombPos, Vector3.forward);
                        float dropAngle = Vector3.Angle(rb.transform.InverseTransformPoint(curBomb.BombPosition), Vector3.forward);
                        //print(cameraPitch);
                        float bombRatio = Mathf.Abs(Mathf.Sin((pitchAngle - dropAngle) * Mathf.Deg2Rad));
                        SetScale(curBomb.Marker.transform, 200f / relBombPos.magnitude, bombRatio);
                    }
                    //print(relBombPos.magnitude);
                }
                else
                {
                    //curBomb.Marker.SetActive(false);
                }
            }
        }
        if (nextTarget != null)
        {
            tNextTarget.SetActive(target != nextTarget && Camera.main.WorldToScreenPoint(nextTarget.transform.position).z > 0);
        }
        else
        {
            tNextTarget.SetActive(false);
        }

        //Set bearing text and position
        textBearing.transform.eulerAngles = new Vector3(0, 0, -cameraRoll);
        textBearing.transform.position = new Vector3(bearingX, bearingY, 0);
        textBearing.text = ((int)(-yawAngle + 360) % 360).ToString();

        //Set Speed and Altitude text
        textSpeed.text = "   SPEED   \n" + ((int)(tSpeed * 100)).ToString();
        textAltitude.text = "   ALTITUDE   \n" + ((int)(tAltitude * 10)).ToString();

        stall.SetActive(tSpeed < stallSpeed + 3f);
        if (tSpeed < stallSpeed + 3f && !stallSFX.isPlaying)
        {
            stallSFX.Play();
        }
        psm.SetActive(Input.GetKey(KeyCode.Space));
    }

    public float RelAngle(float inAngle)
    {
        //Turn absolute angle (0 to 360) into relative angle (-180 to 180)
        float outAngle;
        if (inAngle > 180)
        {
            outAngle = inAngle - 360;
        }
        else
        {
            outAngle = inAngle;
        }
        return outAngle;
    }

    void SetScale(Transform myTrans, float scaleUI, float aspectRatio)
    {
        //find the aspect ratio
        //widthHeightRatio = (float)Screen.width / Screen.height;
        float screenScale = (float)Screen.width / 2000f;

        //print(Screen.width);
        //Apply the scale. We only calculate y since our aspect ratio is x (width) authoritative: width/height (x/y)
        myTrans.localScale = new Vector3(scaleUI * screenScale, scaleUI * screenScale * aspectRatio, 1);
    }

    void SetTextFlow(Text currText)
    {
        //Sets text overflow
        currText.horizontalOverflow = HorizontalWrapMode.Overflow;
        currText.verticalOverflow = VerticalWrapMode.Overflow;
    }

    void CountAllies()
    {
        ally = GameObject.FindGameObjectsWithTag("Ally");
        tAlly = new GameObject[ally.Length];
        mapAlly = new GameObject[ally.Length];

        //Initialize ally markers
        for (int i = 0; i < ally.Length; i++)
        {
            tAlly[i] = new GameObject("ally");
            tAlly[i].transform.SetParent(goCanvas.transform);
            Text textAlly = tAlly[i].AddComponent<Text>();
            textAlly.transform.position = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
            textAlly.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            textAlly.fontSize = 100;
            textAlly.color = Color.cyan;
            textAlly.alignment = TextAnchor.MiddleCenter;
            textAlly.rectTransform.sizeDelta = new Vector2(60, 60);
            textAlly.text = box;

            mapAlly[i] = new GameObject("mapally");
            mapAlly[i].transform.SetParent(minimap.transform.Find("MapElements").transform);
            Text textMapAlly = mapAlly[i].AddComponent<Text>();
            textMapAlly.transform.localPosition = new Vector3(0f, 0f, 0);
            textMapAlly.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            textMapAlly.fontSize = 60;
            textMapAlly.color = Color.cyan;
            textMapAlly.alignment = TextAnchor.MiddleCenter;
            textMapAlly.rectTransform.sizeDelta = new Vector2(20, 20);
            textMapAlly.transform.localScale = new Vector3(0.35f, 0.5f, 0.5f);
            textMapAlly.text = "▲";
            SetTextFlow(textMapAlly);
        }
    }

    void CountEnemies()
    {
        //Initialize enemy markers
        enemy = GameObject.FindGameObjectsWithTag("Enemy");
        //tEnemy = new List<GameObject>();
        tEnemy = new GameObject[enemy.Length];
        mapEnemy = new GameObject[enemy.Length];

        for (int i = 0; i < enemy.Length; i++)
        {
            tEnemy[i] = new GameObject("enemy");
            tEnemy[i].transform.SetParent(goCanvas.transform);
            Text textEnemy = tEnemy[i].AddComponent<Text>();
            textEnemy.transform.position = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
            textEnemy.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            textEnemy.fontSize = 100;
            textEnemy.color = Color.green;
            textEnemy.alignment = TextAnchor.MiddleCenter;
            textEnemy.rectTransform.sizeDelta = new Vector2(60, 60);
            textEnemy.text = box;
            textEnemy.lineSpacing = 0f;

            mapEnemy[i] = new GameObject("mapenemy");
            mapEnemy[i].transform.SetParent(minimap.transform.Find("MapElements").transform);
            Text textMapEnemy = mapEnemy[i].AddComponent<Text>();
            textMapEnemy.transform.localPosition = new Vector3(0f, 0f, 0);
            textMapEnemy.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
            textMapEnemy.fontSize = 60;
            textMapEnemy.color = Color.green;
            textMapEnemy.alignment = TextAnchor.MiddleCenter;
            textMapEnemy.rectTransform.sizeDelta = new Vector2(20, 20);
            if (enemy[i].GetComponent<NameTag>().air)
            {
                textMapEnemy.transform.localScale = new Vector3(0.35f, 0.5f, 0.5f);
                textMapEnemy.text = "▲";
            }
            else
            {
                textMapEnemy.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
                textMapEnemy.text = "■";
            }
            
            SetTextFlow(textMapEnemy);
        }
    }

    void Recount(GameObject testEnemy)
    {

        //Even with the gameobjects deleted I still gotta shorten these arrays
        //Perhaps I should have used lists
        GameObject[] temp1 = new GameObject[enemy.Length - 1];
        GameObject[] temp2 = new GameObject[tEnemy.Length - 1];
        GameObject[] temp3 = new GameObject[mapEnemy.Length - 1];

        bool deleteLast = true;
        for (int i = 0; i + 1 < enemy.Length; i++)
        {
            if (enemy[i] == testEnemy)
            {
                deleteLast = false;
                enemy[i] = enemy[enemy.Length - 1];
                Destroy(tEnemy[i]);
                tEnemy[i] = tEnemy[tEnemy.Length - 1];
                Destroy(mapEnemy[i]);
                mapEnemy[i] = mapEnemy[mapEnemy.Length - 1];
                //break;
            }
            temp1[i] = enemy[i];
            temp2[i] = tEnemy[i];
            temp3[i] = mapEnemy[i];
        }

        if (deleteLast)
        {
            Destroy(tEnemy[tEnemy.Length - 1]);
            Destroy(mapEnemy[mapEnemy.Length - 1]);
        }

        enemy = temp1;
        tEnemy = temp2;
        mapEnemy = temp3;

        tarLock.SetActive(testEnemy.GetComponent<Rigidbody>() != target);
        if (testEnemy.GetComponent<Rigidbody>() == target)
        {
            lockingSFX.Stop();
            lockedSFX.Stop();
        }
        //CountAllies();
        //CountEnemies();
        //print("Something was shot down");
    }

    void AddWarning(GameObject missile)
    {
        enemyMissile.Add(missile);
        
        //print("Yo it works");
        missilePointer.Add(new GameObject("MissilePointer"));
        missilePointer[missilePointer.Count-1].transform.SetParent(goCanvas.transform);
        Text textMissPointer = missilePointer[missilePointer.Count - 1].AddComponent<Text>();
        textMissPointer.transform.position = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
        textMissPointer.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
        textMissPointer.fontSize = 140;
        textMissPointer.color = Color.red;
        textMissPointer.alignment = TextAnchor.MiddleCenter;
        textMissPointer.rectTransform.sizeDelta = new Vector2(60, 60);
        textMissPointer.text = solidArrow;

        SetScale(missilePointer[missilePointer.Count - 1].transform, scaleUI, 1f);
        SetTextFlow(textMissPointer);

        //print(missilePointer.Count);
        UpdateWarning();
    }

    void SubWarning(GameObject missile)
    {
        //int index = enemyMi
        //print("removing");
        enemyMissile.Remove(missile);
        enemyMissile.TrimExcess();

        //missilePointer[missilePointer.Count - 1] = null;
        GameObject temp = missilePointer[missilePointer.Count - 1];
        missilePointer.RemoveAt(missilePointer.Count - 1);
        Destroy(temp);
        missilePointer.TrimExcess();

        //print(missilePointer.Count);
        UpdateWarning();
    }

    void CountSpikes(int currSpike)
    {
        spike += currSpike;
        //print(spike);

        UpdateWarning();
    }

    void UpdateWarning()
    {
        warning.SetActive((spike > 0) || (enemyMissile.Count > 0));
        if (enemyMissile.Count > 0)
        {
            
            
            
            textWarning.text = "[MISSILE]";
        }
        else
        {
            //missileSFX.Stop();
            textWarning.text = "[WARNING]";
        }
    }

    void AddBomb(GameObject thisBomb)
    {
        
        DroppedBomb dBomb = new DroppedBomb();

        GameObject bombMarker = new GameObject("BombMarker");
        bombMarker.transform.SetParent(goCanvas.transform);
        Text textBombMarker = bombMarker.AddComponent<Text>();
        textBombMarker.transform.position = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
        textBombMarker.font = Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
        textBombMarker.fontSize = 300;
        textBombMarker.color = Color.red;
        textBombMarker.alignment = TextAnchor.MiddleCenter;
        textBombMarker.text = circle;
        SetTextFlow(textBombMarker);
        bombMarker.SetActive(false);
        dBomb.Marker = bombMarker;
        dBomb.BombPosition = bombPos;
        dBomb.BombObject = thisBomb;
        droppedBombs.Add(dBomb);
    }

    void SubBomb(GameObject checkBomb)
    {
        /*
        foreach(DroppedBomb curBomb in droppedBombs)
        {
            if(curBomb.BombObject == checkBomb)
            {
                droppedBombs.Remove(curBomb);
                droppedBombs.TrimExcess();
            }
        }
        */
        for (int i = 0; i < droppedBombs.Count; i++)
        {
            if(droppedBombs[i].BombObject == checkBomb)
            {
                GameObject temp = droppedBombs[i].Marker;
                
                droppedBombs.RemoveAt(i);
                
                Destroy(temp);
                droppedBombs.TrimExcess();
                break;
            }
        }
    }

    void UpdateTarget(Rigidbody mainTarget)
    {
        target = mainTarget;
        //target = rb.GetComponent<PlaneDriver>().target;
    }

    void UpdateNextTarget(Rigidbody nextTarg)
    {
        nextTarget = nextTarg;
    }

    void UpdateSP(int mode)
    {
        spMode = mode;

        foreach (GameObject missileWep in missileLoad)
        {
            missileWep.SetActive(spMode == 1);
        }

        foreach (GameObject spWep in spLoad)
        {
            spWep.SetActive(spMode == 2);
        }
    }

    RaycastHit GravCast(Vector3 startPos, Vector3 velocity, int killAfter, float timeStep = .05f)
    {
        RaycastHit hit;

        Vector3[] vectors = new Vector3[killAfter];
        Ray ray = new Ray(startPos, velocity);

        velocity.y += (Physics.gravity.y * timeStep);
        float length = velocity.magnitude * timeStep;

        for (int i = 0; i < killAfter; i++)
        {
            if (Physics.Raycast(ray, out hit, length))
            {
                //return vectors[i - 1];
                if ((hit.collider.tag != "Bomb") && (hit.collider.tag != "Bullet") && 
                    (hit.collider.tag != "Player") && (hit.collider.tag != "Missile"))
                {
                    bombTrajectory.positionCount = i;
                    bombTrajectory.SetPositions(vectors);
                    return hit;
                }
                
            }
            Debug.DrawRay(ray.origin, ray.direction * length, Color.green);
            velocity.y += (Physics.gravity.y * timeStep);

            ray = new Ray(ray.origin + ray.direction * length, velocity * timeStep);
            length = velocity.magnitude * timeStep;
            //ray = new Ray(ray.origin + ray.direction * length, length * (ray.direction + (Physics.gravity / speed)));
            vectors[i] = ray.origin;

        }

        Physics.Raycast(ray, out hit, 300f);
        Debug.DrawRay(ray.origin, ray.direction * 300, Color.blue);
        //backupBomb = ray.origin;
        return hit;
    }

}

public struct DroppedBomb
{
    public GameObject Marker;
    public GameObject BombObject;
    public Vector3 BombPosition;
}
