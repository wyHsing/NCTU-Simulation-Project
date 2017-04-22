using UnityEngine;
using System.Collections;

public class fire_spread : MonoBehaviour
{
    public GameObject[] particlePrefabs;
    [Tooltip("半径越小,模型越密集")]
    public float radius = 0.5f;
    [Tooltip("高度越大，模型能越过更高的遮挡物")]
    public float height = 0.5f;
    [Tooltip("层级越多，向外扩展的导数越多")]
    public int level = 10;
    [Tooltip("每层间隔生成时间")]
    public float deltaTime = 5.0f;
    [Tooltip("控制火焰显示时间")]
    public float duration = 100.0f;
    [Tooltip("水平偏移")]
    public float offsetHeight = 0.1f;
    [Tooltip("高度偏移")]
    public float offsetPanel = 0.2f;
    [Tooltip("模型与地面的距离")]
    public float prefabHeight = 0.01f;

    private int count;
    private float angle;

#if UNITY_EDITOR  
    public IEnumerator Start()
    {
        yield return new WaitForSeconds(5.0f);
        extend();
    }     
    public void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.K))
        {
            extend();
        }*/
    }
#endif

    public void attack(int level)
    {
        this.level = level;
        extend();
    }

    //分成多条线向外扩散
    public void extend()
    {
        //分成多条线
        count = (int)(2 * level * Mathf.PI);
        //每条偏移角度
        angle = 360.0f / count;
        var up = transform.rotation * Vector3.up;
        for (int i = 0; i < count; i++)
        {
            var curAngle = angle * i;
            var curQuat = Quaternion.AngleAxis(curAngle, up);
            var quat = curQuat * transform.rotation;
            //forward(transform.position, quat, level, i);
            StartCoroutine(forward(transform.position, quat, level, i));
        }
    }

    //每条线向前扩展，自动翻越遮挡
    public IEnumerator forward(Vector3 pos, Quaternion quat, int level, int place)
    {
        var npos = pos;
        var nquat = quat;
        int curLevel = 1;

        var prePos = npos;
        while (curLevel <= level && forward(ref npos, ref nquat))
        {
            //Debug.DrawLine(prePos, npos, Color.blue, 100f, true);
            if (bCreate(curLevel, place))
            {
                var offset = offsetPos(nquat, 1);
                var obj = Instantiate(getPrefab(curLevel), npos + offset, nquat);
                //Debug.DrawLine(prePos, npos, Color.blue, 100f, true);
                var duration = getDuration(curLevel);
                Destroy(obj, duration);
            }
            yield return new WaitForSeconds(deltaTime);
            curLevel++;
            prePos = npos;
        }
    }

    //确定当前层上的某个位置是否需要生成
    bool bCreate(int level, int place)
    {
        //当前需要显示的个数
        var levelCount = (int)(2 * level * Mathf.PI);
        var levelLength = Mathf.RoundToInt((float)count / levelCount);
        //每层起点偏移一点位置
        var offset = place + level;
        return offset % levelLength == 0;
    }
    public GameObject getPrefab(int curLevel)
    {
        var prfabCount = particlePrefabs.Length;
        //var t = Mathf.RoundToInt(prfabCount * (curLevel - 1) / level);
        var index = Random.Range(0, prfabCount);
        return particlePrefabs[index];
    }

    //加上偏移
    public Vector3 offsetPos(Quaternion quat, int level)
    {
        var xAxis = quat * Vector3.right * Random.Range(-offsetPanel, offsetPanel) * level;
        var zAxis = quat * Vector3.forward * Random.Range(-offsetPanel, offsetPanel) * level;
        var yAxis = quat * Vector3.up * Random.Range(-offsetHeight, offsetHeight) * level;
        return xAxis + zAxis + yAxis;
    }

    //每层的生命周期
    float getDuration(int level)
    {
        var levelDuration = duration; //+ level * deltaTime;
        return levelDuration;
    }

    //根据一条线的上一个节点，确定这个节点如何定位
    bool forward(ref Vector3 pos, ref Quaternion quat)
    {
        var forward = quat * Vector3.forward;
        var up = quat * Vector3.up;

        var nextPos = pos + forward * radius;
        RaycastHit hit;
        //前面有没挡住
        //Debug.DrawRay(nextPos, forward, Color.red, 100);
        var hitForward = Physics.Raycast(nextPos, forward, out hit, radius * 2);
        var hitDown = false;
        //如果前面没有挡住,检查垂直位置
        if (!hitForward)
        {
            //height用来控制垂直扩展的高度
            var upPos = pos + 2 * forward * radius + up * height * 2;
            //Debug.DrawRay(upPos, -up, Color.green, 100);
            hitDown = Physics.Raycast(upPos, -up, out hit, height * 4);
        }
        if (hitForward || hitDown)
        {
            //新的位置
            pos = hit.point + hit.normal * prefabHeight;
            //Debug.DrawRay(pos, hit.normal, Color.blue, 100);
            quat = Quaternion.FromToRotation(up, hit.normal) * quat;
            return true;
        }
        return false;
    }
}