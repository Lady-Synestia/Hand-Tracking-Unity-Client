using UnityEngine;

public class Hand : MonoBehaviour
{
    private Transform[] _childTransforms;

    // scalar for raw position data
    private readonly Vector3 _scalar = new Vector3(1, -1, 1);

    private void Start()
    {
        // number of children is expected to be 21
        // storing transforms of children so they don't have to be found again later
        _childTransforms = new Transform[this.transform.childCount];
        for (int i = 0; i < this.transform.childCount; i++)
        {
            _childTransforms[i] = this.transform.GetChild(i);
        }
    }

    public void SetChildPosition(int childIndex, Vector3 position)
    {
        _childTransforms[childIndex].position = Vector3.Scale(position, _scalar);
    }
}
