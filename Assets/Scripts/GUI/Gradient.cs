using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;


[AddComponentMenu("UI/Effects/Gradient")]
public class Gradient : BaseMeshEffect {
    [SerializeField]
    private Color32 topColor = Color.white;
    [SerializeField]
    private Color32 bottomColor = Color.black;

	public override void ModifyMesh (VertexHelper vh)
	{
		if (!this.IsActive())
			return;
		
		List<UIVertex> vertexList  = new List<UIVertex>();
		vh.GetUIVertexStream(vertexList);
		
		ModifyVertices(vertexList);  // calls the old ModifyVertices which was used on pre 5.2
		
		vh.Clear();
		vh.AddUIVertexTriangleStream(vertexList);
	}

	public void ModifyVertices(List<UIVertex> vertexList) {
        if (!IsActive() || vertexList.Count == 0) {
            return;
        }		
        int count = vertexList.Count;
        float bottomY = vertexList[0].position.y;
        float topY = vertexList[0].position.y;        

        for (int i = 1; i < count; i++) {
            float y = vertexList[i].position.y;
            if (y > topY) {
                topY = y;
            }
            else if (y < bottomY) {
                bottomY = y;
            }            
        }

        float uiElementHeight = topY - bottomY;
        //float uiElementWidth = topX - bottomX;

        for (int i = 0; i < count; i++) {
            UIVertex uiVertex = vertexList[i];
            uiVertex.color = Color32.Lerp(bottomColor, topColor, (uiVertex.position.y - bottomY) / uiElementHeight);            
            vertexList[i] = uiVertex;
        }
    }
}
