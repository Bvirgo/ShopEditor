using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(GridLayoutGroup))]
public class InfinityGridLayoutGroup : MonoBehaviour 
{
    #region Container
    RectTransform m_gridRtf;
    GridLayoutGroup m_gridLayoutGroup;
    ScrollRect m_scrollRect;
    List<RectTransform> m_pRealChildren;

    Vector2 m_gridLayoutSize;
    Vector2 m_vGridLayoutPos;
    Dictionary<Transform, Vector2> childsAnchoredPosition;
    Dictionary<Transform, int> childsSiblingIndex;

    private Action<int, Transform> m_actUpdate;

    int m_nRealCount = 0;
    Vector2 m_vStartPos;
    int m_nMaxCount = 0;

    int m_nRealIndex = -1;
    bool m_bInit = false;
    #endregion

    #region Init
    IEnumerator InitChildren()
    {
        yield return 0;

        if (!m_bInit)
        {
            //获取Grid的宽度;
            m_gridRtf = GetComponent<RectTransform>();

            m_gridLayoutGroup = GetComponent<GridLayoutGroup>();
            m_gridLayoutGroup.enabled = false;

            childsAnchoredPosition = new Dictionary<Transform, Vector2>();
            childsSiblingIndex = new Dictionary<Transform, int>();
            m_pRealChildren = new List<RectTransform>();

            m_vGridLayoutPos = m_gridRtf.anchoredPosition;
            m_gridLayoutSize = m_gridRtf.sizeDelta;

            //注册ScrollRect滚动回调;
            m_scrollRect = transform.parent.GetComponent<ScrollRect>();
            m_scrollRect.onValueChanged.AddListener((data) => { ScrollCallback(data); });

            //获取所有child anchoredPosition 以及 SiblingIndex;
            for (int index = 0; index < transform.childCount; index++)
            {
                Transform child = transform.GetChild(index);
                RectTransform childRectTrans = child.GetComponent<RectTransform>();
                childsAnchoredPosition.Add(child, childRectTrans.anchoredPosition);
                childsSiblingIndex.Add(child, child.GetSiblingIndex());
            }
            m_nRealCount = transform.childCount;
        }
        else
        {
            m_gridRtf.anchoredPosition = m_vGridLayoutPos;
            m_gridRtf.sizeDelta = m_gridLayoutSize;

            m_pRealChildren.Clear();
            m_nRealIndex = -1;
            //children重新设置上下顺序;
            foreach (var info in childsSiblingIndex)
            {
                info.Key.SetSiblingIndex(info.Value);
            }

            //children重新设置anchoredPosition;
            for (int index = 0; index < transform.childCount; index++)
            {
                Transform child = transform.GetChild(index);

                RectTransform childRectTrans = child.GetComponent<RectTransform>();
                if (childsAnchoredPosition.ContainsKey(child))
                {
                    childRectTrans.anchoredPosition = childsAnchoredPosition[child];
                }
                else
                {
                    Debug.LogError("childsAnchoredPosition no contain " + child.name);
                }
            }
        }

        //获取所有child;
        for (int index = 0; index < transform.childCount; index++)
        {
            Transform trans = transform.GetChild(index);
            trans.gameObject.SetActive(true);

            m_pRealChildren.Add(transform.GetChild(index).GetComponent<RectTransform>());

            //初始化前面几个;
            UpdateChildrenCallback(m_pRealChildren.Count - 1, transform.GetChild(index));
        }

        m_vStartPos = m_gridRtf.anchoredPosition;

        m_nRealIndex = m_pRealChildren.Count - 1;

        //Debug.Log( scrollRect.transform.TransformPoint(Vector3.zero));

        // Debug.Log(transform.TransformPoint(children[0].localPosition));

        m_bInit = true;

        //如果需要显示的个数小于设定的个数;
        for (int index = 0; index < m_nRealCount; index++)
        {
            m_pRealChildren[index].gameObject.SetActive(index < m_nMaxCount);
        }

        if (m_gridLayoutGroup.constraint == GridLayoutGroup.Constraint.FixedColumnCount)
        {
            //如果小了一行，则需要把GridLayout的高度减去一行的高度;
            int row = (m_nRealCount - m_nMaxCount) / m_gridLayoutGroup.constraintCount;
            if (row > 0)
            {
                m_gridRtf.sizeDelta -= new Vector2(0, (m_gridLayoutGroup.cellSize.y + m_gridLayoutGroup.spacing.y) * row);
            }
        }
        else
        {
            //如果小了一列，则需要把GridLayout的宽度减去一列的宽度;
            int column = (m_nRealCount - m_nMaxCount) / m_gridLayoutGroup.constraintCount;
            if (column > 0)
            {
                m_gridRtf.sizeDelta -= new Vector2((m_gridLayoutGroup.cellSize.x + m_gridLayoutGroup.spacing.x) * column, 0);
            }
        }
    }

    /// <summary>
    /// 滑动回调
    /// </summary>
    /// <param name="data"></param>
    void ScrollCallback(Vector2 data)
    {
        UpdateChildren();
    }

    void UpdateChildrenCallback(int index, Transform trans)
    {
        if (m_actUpdate != null)
        {
            m_actUpdate(index, trans);
        }
    }
    #endregion

    #region Scroll List 
    void UpdateChildren()
    {
        if (transform.childCount < m_nRealCount)
        {
            return;
        }

        Vector2 currentPos = m_gridRtf.anchoredPosition;

        if (m_gridLayoutGroup.constraint == GridLayoutGroup.Constraint.FixedColumnCount)
        {
            float offsetY = currentPos.y - m_vStartPos.y;

            if (offsetY > 0)
            {
                //向上拉，向下扩展;
                {
                    if (m_nRealIndex >= m_nMaxCount - 1)
                    {
                        m_vStartPos = currentPos;
                        return;
                    }
                    else
                    {
                        MoveTop2Bottom();
                    }
                }
            }
            else
            {
                //Debug.Log("Drag Down");
                //向下拉，下面收缩;
                if (m_nRealIndex + 1 <= m_pRealChildren.Count)
                {
                    m_vStartPos = currentPos;
                    return;
                }
                else
                {
                    MoveBottom2Top();
                }

            }
        }
        else
        {
            float offsetX = currentPos.x - m_vStartPos.x;

            if (offsetX < 0)
            {
                //向左拉，向右扩展;
                {
                    if (m_nRealIndex >= m_nMaxCount - 1)
                    {
                        m_vStartPos = currentPos;
                        return;
                    }
                    else
                    {
                        MoveLeft2Right();
                    }
                }
            }
            else
            {
                //Debug.Log("Drag Down");
                //向右拉，右边收缩;
                if (m_nRealIndex + 1 <= m_pRealChildren.Count)
                {
                    m_vStartPos = currentPos;
                    return;
                }
                else
                {
                    MoveRight2Left();
                }

            }
        }

        m_vStartPos = currentPos;
    }

    void MoveTop2Bottom()
    {
        float scrollRectUp = m_scrollRect.transform.TransformPoint(Vector3.zero).y;

        Vector3 childBottomLeft = new Vector3(m_pRealChildren[0].anchoredPosition.x, m_pRealChildren[0].anchoredPosition.y - m_gridLayoutGroup.cellSize.y, 0f);
        float childBottom = transform.TransformPoint(childBottomLeft).y;

        if (childBottom >= scrollRectUp)
        {
            //Debug.Log("childBottom >= scrollRectUp");

            //移动到底部;
            for (int index = 0; index < m_gridLayoutGroup.constraintCount; index++)
            {
                m_pRealChildren[index].SetAsLastSibling();

                m_pRealChildren[index].anchoredPosition = new Vector2(m_pRealChildren[index].anchoredPosition.x, m_pRealChildren[m_pRealChildren.Count - 1].anchoredPosition.y - m_gridLayoutGroup.cellSize.y - m_gridLayoutGroup.spacing.y);

                m_nRealIndex++;

                if (m_nRealIndex > m_nMaxCount - 1)
                {
                    m_pRealChildren[index].gameObject.SetActive(false);
                }
                else
                {
                    UpdateChildrenCallback(m_nRealIndex, m_pRealChildren[index]);
                }
            }

            //GridLayoutGroup 底部加长;
            m_gridRtf.sizeDelta += new Vector2(0, m_gridLayoutGroup.cellSize.y + m_gridLayoutGroup.spacing.y);

            //更新child;
            for (int index = 0; index < m_pRealChildren.Count; index++)
            {
                m_pRealChildren[index] = transform.GetChild(index).GetComponent<RectTransform>();
            }
        }
    }

    void MoveBottom2Top()
    {
        RectTransform scrollRectTransform = m_scrollRect.GetComponent<RectTransform>();
        Vector3 scrollRectAnchorBottom = new Vector3(0, -scrollRectTransform.rect.height - m_gridLayoutGroup.spacing.y, 0f);
        float scrollRectBottom = m_scrollRect.transform.TransformPoint(scrollRectAnchorBottom).y;

        Vector3 childUpLeft = new Vector3(m_pRealChildren[m_pRealChildren.Count - 1].anchoredPosition.x, m_pRealChildren[m_pRealChildren.Count - 1].anchoredPosition.y, 0f);

        float childUp = transform.TransformPoint(childUpLeft).y;

        if (childUp < scrollRectBottom)
        {
            //Debug.Log("childUp < scrollRectBottom");

            //把底部的一行 移动到顶部
            for (int index = 0; index < m_gridLayoutGroup.constraintCount; index++)
            {
                m_pRealChildren[m_pRealChildren.Count - 1 - index].SetAsFirstSibling();

                m_pRealChildren[m_pRealChildren.Count - 1 - index].anchoredPosition = new Vector2(m_pRealChildren[m_pRealChildren.Count - 1 - index].anchoredPosition.x, m_pRealChildren[0].anchoredPosition.y + m_gridLayoutGroup.cellSize.y + m_gridLayoutGroup.spacing.y);

                m_pRealChildren[m_pRealChildren.Count - 1 - index].gameObject.SetActive(true);

                UpdateChildrenCallback(m_nRealIndex - m_pRealChildren.Count - index, m_pRealChildren[m_pRealChildren.Count - 1 - index]);
            }

            m_nRealIndex -= m_gridLayoutGroup.constraintCount;

            //GridLayoutGroup 底部缩短;
            m_gridRtf.sizeDelta -= new Vector2(0, m_gridLayoutGroup.cellSize.y + m_gridLayoutGroup.spacing.y);

            //更新child;
            for (int index = 0; index < m_pRealChildren.Count; index++)
            {
                m_pRealChildren[index] = transform.GetChild(index).GetComponent<RectTransform>();
            }
        }
    }

    void MoveLeft2Right()
    {
        float scrollRectLeft = m_scrollRect.transform.TransformPoint(Vector3.zero).x;

        Vector3 childBottomRight = new Vector3(m_pRealChildren[0].anchoredPosition.x + m_gridLayoutGroup.cellSize.x, m_pRealChildren[0].anchoredPosition.y, 0f);
        float childRight = transform.TransformPoint(childBottomRight).x;

        // Debug.LogError("childRight=" + childRight);

        if (childRight <= scrollRectLeft)
        {
            //Debug.Log("childRight <= scrollRectLeft");

            //移动到右边;
            for (int index = 0; index < m_gridLayoutGroup.constraintCount; index++)
            {
                m_pRealChildren[index].SetAsLastSibling();

                m_pRealChildren[index].anchoredPosition = new Vector2(m_pRealChildren[m_pRealChildren.Count - 1].anchoredPosition.x + m_gridLayoutGroup.cellSize.x + m_gridLayoutGroup.spacing.x, m_pRealChildren[index].anchoredPosition.y);

                m_nRealIndex++;

                if (m_nRealIndex > m_nMaxCount - 1)
                {
                    m_pRealChildren[index].gameObject.SetActive(false);
                }
                else
                {
                    UpdateChildrenCallback(m_nRealIndex, m_pRealChildren[index]);
                }
            }

            //GridLayoutGroup 右侧加长;
            m_gridRtf.sizeDelta += new Vector2(m_gridLayoutGroup.cellSize.x + m_gridLayoutGroup.spacing.x, 0);

            //更新child;
            for (int index = 0; index < m_pRealChildren.Count; index++)
            {
                m_pRealChildren[index] = transform.GetChild(index).GetComponent<RectTransform>();
            }
        }
    }

    void MoveRight2Left()
    {
        RectTransform scrollRectTransform = m_scrollRect.GetComponent<RectTransform>();
        Vector3 scrollRectAnchorRight = new Vector3(scrollRectTransform.rect.width + m_gridLayoutGroup.spacing.x, 0, 0f);
        float scrollRectRight = m_scrollRect.transform.TransformPoint(scrollRectAnchorRight).x;

        Vector3 childUpLeft = new Vector3(m_pRealChildren[m_pRealChildren.Count - 1].anchoredPosition.x, m_pRealChildren[m_pRealChildren.Count - 1].anchoredPosition.y, 0f);

        float childLeft = transform.TransformPoint(childUpLeft).x;

        if (childLeft >= scrollRectRight)
        {
            //Debug.LogError("childLeft > scrollRectRight");

            //把右边的一行 移动到左边;
            for (int index = 0; index < m_gridLayoutGroup.constraintCount; index++)
            {
                m_pRealChildren[m_pRealChildren.Count - 1 - index].SetAsFirstSibling();

                m_pRealChildren[m_pRealChildren.Count - 1 - index].anchoredPosition = new Vector2(m_pRealChildren[0].anchoredPosition.x - m_gridLayoutGroup.cellSize.x - m_gridLayoutGroup.spacing.x, m_pRealChildren[m_pRealChildren.Count - 1 - index].anchoredPosition.y);

                m_pRealChildren[m_pRealChildren.Count - 1 - index].gameObject.SetActive(true);

                UpdateChildrenCallback(m_nRealIndex - m_pRealChildren.Count - index, m_pRealChildren[m_pRealChildren.Count - 1 - index]);
            }

            //GridLayoutGroup 右侧缩短;
            m_gridRtf.sizeDelta -= new Vector2(m_gridLayoutGroup.cellSize.x + m_gridLayoutGroup.spacing.x, 0);

            //更新child;
            for (int index = 0; index < m_pRealChildren.Count; index++)
            {
                m_pRealChildren[index] = transform.GetChild(index).GetComponent<RectTransform>();
            }

            m_nRealIndex -= m_gridLayoutGroup.constraintCount;
        }
    }
    #endregion

    #region Public

    /// <summary>
    /// Set Virtual List Max Count
    /// </summary>
    /// <param name="count"></param>
    public void SetAmount(int count)
    {
        m_nMaxCount = count;

        StartCoroutine(InitChildren());
    }

    /// <summary>
    /// Register Call Back For Update Child
    /// </summary>
    /// <param name="_actUpdate"></param>
    public void Register(Action<int,Transform> _actUpdate)
    {
        m_actUpdate = _actUpdate;
    }
    #endregion
}
