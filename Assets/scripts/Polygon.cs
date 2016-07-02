using UnityEngine;
using System.Collections;

public class Polygon {
	Vector2[] points;
	
	public Polygon(Vector2[] points) {
		this.points = points;
	}
	
	int getValidIndex(int idx, int offset, int bound) {
		return (int)(Mathf.Abs(idx-offset) % bound);
	}
	
	public Vector2 GetRandomPointFromInside() {
		Vector2 point;
		if (points.Length < 3) return Vector2.zero;
		
		//find random vertex
		int centerIndex = Random.Range(0, points.Length);
		//now generate left and right offset from centerVertex
		
		float leftCoeff = Random.Range(0.0f, 1.0f);
		float rightCoeff = Random.Range(0.0f, 1.0f);
		
		if (leftCoeff + rightCoeff > 1.0) {
			leftCoeff = 1.0f - leftCoeff;
			rightCoeff = 1.0f - rightCoeff;
		}
		
		float centerCoeff = 1.0f - leftCoeff - rightCoeff;
		
		Vector2 centerVertex = points[centerIndex];
		Vector2 leftSide = points[getValidIndex(centerIndex, -1, points.Length)];
		Vector2 rightSide = points[getValidIndex(centerIndex, 1, points.Length)];
		
		leftSide *= leftCoeff;
		rightSide *= rightCoeff;
		centerVertex *= centerCoeff;
		point = leftSide + rightSide + centerVertex;
		
		return point;
	}
}
