using UnityEngine;
using UnityEngine.UI;

public class MenuAnimationController : MonoBehaviour
{
    public static MenuAnimationController instance;

    [SerializeField] Transform note4, note8, note16;
    [SerializeField] int noteCount;
    [SerializeField] float minSpeed, maxSpeed;
    [SerializeField] Transform noteContainer;
    [SerializeField] EasingFunc easingFunction;

    float minScale = 0.1f, maxScale = 1f;
    Vector3 spriteSize;

    (Transform transform, float speed)[] notes;

    public float speedMultiplier = 1.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        notes = new (Transform, float)[noteCount];
    }

    private void Start()
    {
        spriteSize = note8.GetComponent<Image>().sprite.rect.size;

        Transform[] noteOptions = { note4, note8, note16 };

        for (int i = 0; i < noteCount; i++)
        {
            int whichNote = Random.Range(0, 3);
            Transform n = Instantiate(noteOptions[whichNote]);
            float speed = Random.Range(minSpeed, maxSpeed);

            (float easedSpeed, float easedScale) = EaseSpeedAndScale(easingFunction, speed);

            n.localScale = easedScale * Vector3.one;
            n.localPosition = RandomStartingPoint();
            n.SetParent(noteContainer);

            notes[i].transform = n;
            notes[i].speed = easedSpeed;
        }
    }

    (float, float) EaseSpeedAndScale(EasingFunc fn, float speed)
    {
        float speedT = (speed - minSpeed) / (maxSpeed - minSpeed);
        float easedT = Easing(fn, speedT);

        float easedSpeed = Mathf.Lerp(minSpeed, maxSpeed, easedT);
        float easedScale = Mathf.Lerp(minScale, maxScale, easedT);

        return (easedSpeed, easedScale);
    }

    Vector3 RandomStartingPoint()
    {
        float x = Random.Range(0, Screen.width);
        float y = Random.Range(0, Screen.height);
        return new Vector3(x, y);
    }

    // Update is called once per frame
    void Update()
    {
        foreach(var n in notes)
        {
            int offset = (int)(spriteSize.x * n.transform.localScale.x);
            if (n.transform.position.x > Screen.width + offset)
            {
                n.transform.position = new Vector3(-offset, n.transform.position.y);
            }
            n.transform.position += speedMultiplier * Time.deltaTime * new Vector3(n.speed, 0, 0);
        }
    }

    float Easing(EasingFunc func, float x)
    {
        return func switch
        {
            EasingFunc.Linear => x,
            EasingFunc.EaseInQuad => x * x,
            EasingFunc.EaseInCubic => x * x * x,
            EasingFunc.EaseInQuart => x * x * x * x,
            EasingFunc.EaseInBounce => 1f - EaseOutBounce(x),
            EasingFunc.EaseOutBounce => EaseOutBounce(x),
            _ => x,
        };
    }

    float EaseOutBounce(float x)
    {
        float n1 = 7.5625f;
        float d1 = 2.75f;

        if (x < 1 / d1)
        {
            return n1 * x * x;
        }
        else if (x < 2 / d1)
        {
            return n1 * (x -= 1.5f / d1) * x + 0.75f;
        }
        else if (x < 2.5 / d1)
        {
            return n1 * (x -= 2.25f / d1) * x + 0.9375f;
        }
        else
        {
            return n1 * (x -= 2.625f / d1) * x + 0.984375f;
        }
    }
}

public enum EasingFunc
{
    Linear,
    EaseInQuad,
    EaseInCubic,
    EaseInQuart,
    EaseInBounce,
    EaseOutBounce
};