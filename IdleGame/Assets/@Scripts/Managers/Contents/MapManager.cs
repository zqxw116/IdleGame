using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;
using static Define;


public class MapManager
{
	public GameObject Map { get; private set; }
	public string MapName { get; private set; }
	public Grid CellGrid { get; private set; }	// 

	// (CellPos, BaseObject)
	// 물체끼리의 충돌을 체크하기 위해 존재
	Dictionary<Vector3Int, BaseObject> _cells = new Dictionary<Vector3Int, BaseObject>(); // 한칸에 오브젝트 하나
	// 갈 수 있는 영역과 갈 수 없는 영역을 가진다
	private int MinX;
	private int MaxX;
	private int MinY;
	private int MaxY;

	// (3.5, 0, 2.7) 같은 월드 위치가 그리드 내 어떤 셀에 있는지 계산함. 
	// 이때 결과는 정수 좌표 (Vector3Int)로 나와서 "몇 번째 칸인지" 알 수 있음
	public Vector3Int World2Cell(Vector3 worldPos) { return CellGrid.WorldToCell(worldPos); }

	// 셀 좌표 (Vector3Int)를 **월드 좌표 (Vector3)**로 변환
	// 예를 들어, (1, 0, 2)라는 셀 위치가 실제 게임 월드의 어디 위치인지 알려줌
	public Vector3 Cell2World(Vector3Int cellPos) { return CellGrid.CellToWorld(cellPos); }


	// 진짜 충돌 정보. 파란 부분
	// 이미 다 그렸으니 hash로 안해도 돼서 2차 배열로 했지만 고민 필요
	ECellCollisionType[,] _collision;


	public void LoadMap(string mapName)
	{
		DestroyMap();

		GameObject map = Managers.Resource.Instantiate(mapName);
		map.transform.position = Vector3.zero;
		map.name = $"@Map_{mapName}";

		Map = map;
		MapName = mapName;
		CellGrid = map.GetComponent<Grid>();

		ParseCollisionData(map, mapName);

	//	SpawnObjectsByData(map, mapName);
	}

	public void DestroyMap()
	{
		ClearObjects();

		if (Map != null)
			Managers.Resource.Destroy(Map);
	}

	void ParseCollisionData(GameObject map, string mapName, string tilemap = "Tilemap_Collision")
	{
		GameObject collision = Util.FindChild(map, tilemap, true);
		if (collision != null) // 파란색이 보이게 되니 있으면 꺼줘야 한다.
			collision.SetActive(false);

		// Collision 관련 파일
		TextAsset txt = Managers.Resource.Load<TextAsset>($"{mapName}Collision");   // /Data/MapData/BaseMapCollision.txt
		StringReader reader = new StringReader(txt.text);

		MinX = int.Parse(reader.ReadLine());
		MaxX = int.Parse(reader.ReadLine());
		MinY = int.Parse(reader.ReadLine());
		MaxY = int.Parse(reader.ReadLine());

		int xCount = MaxX - MinX + 1;
		int yCount = MaxY - MinY + 1;
		_collision = new ECellCollisionType[xCount, yCount];

		for (int y = 0; y < yCount; y++)
		{
			string line = reader.ReadLine();
			for (int x = 0; x < xCount; x++)
			{
				switch (line[x])
				{
					case Define.MAP_TOOL_WALL:		// 비트 : 0
						_collision[x, y] = ECellCollisionType.Wall;
						break;		
					case Define.MAP_TOOL_NONE:		// 비트 : 1
						_collision[x, y] = ECellCollisionType.None;
						break;
					case Define.MAP_TOOL_SEMI_WALL: // 비트 : 2
						_collision[x, y] = ECellCollisionType.SemiWall;
						break;
				}
			}
		}
	}

	//void SpawnObjectsByData(GameObject map, string mapName, string tilemap = "Tilemap_Object")
	//{
	//	Tilemap tm = Util.FindChild<Tilemap>(map, tilemap, true);

	//	if (tm != null)
	//		tm.gameObject.SetActive(false);

	//	for (int y = tm.cellBounds.yMax; y >= tm.cellBounds.yMin; y--)
	//	{
	//		for (int x = tm.cellBounds.xMin; x <= tm.cellBounds.xMax; x++)
	//		{
	//			Vector3Int cellPos = new Vector3Int(x, y, 0);
	//			CustomTile tile = tm.GetTile(cellPos) as CustomTile;
	//			if (tile == null)
	//				continue;

	//			if (tile.ObjectType == Define.EObjectType.Env)
	//			{
	//				Vector3 worldPos = Cell2World(cellPos);
	//				Env env = Managers.Object.Spawn<Env>(worldPos, tile.DataTemplateID);
	//				env.SetCellPos(cellPos, true);
	//			}
	//			else
	//			{
	//				if (tile.CreatureType == Define.ECreatureType.Monster)
	//				{
	//					Vector3 worldPos = Cell2World(cellPos);
	//					Monster monster = Managers.Object.Spawn<Monster>(worldPos, tile.DataTemplateID);
	//					monster.SetCellPos(cellPos, true);
	//				}
	//				else if (tile.CreatureType == Define.ECreatureType.Npc)
	//				{

	//				}
	//			}
	//		}
	//	}
	//}

	/// <summary>
	/// 셀 위치로 이동(생성 빼고는 기본적으로 이 함수로 이동)
	/// </summary>
	public bool MoveTo(Creature obj, Vector3Int cellPos, bool forceMove = false)
	{
		// 1. 셀위치 이동 가능한지 확인(다른 오브젝트 있으면 안함)
		if (CanGo(cellPos) == false)
			return false;

		// 2. 기존 좌표에 있던 오브젝트를 밀어준다.
		// (단, 처음 신청했으면 해당 CellPos의 오브젝트가 본인이 아닐 수도 있음)
		RemoveObject(obj);

		// 3. 새 좌표에 오브젝트를 등록한다.
		AddObject(obj, cellPos);

		// 4. 셀 좌표 이동
		obj.SetCellPos(cellPos, forceMove);

		//Debug.Log($"Move To {cellPos}");

		return true;
	}

	#region Helpers
	public BaseObject GetObject(Vector3Int cellPos)
	{
		// 없으면 null
		_cells.TryGetValue(cellPos, out BaseObject value);
		return value;
	}

	public BaseObject GetObject(Vector3 worldPos)
	{
		Vector3Int cellPos = World2Cell(worldPos);
		return GetObject(cellPos);
	}

	public bool RemoveObject(BaseObject obj)
	{
		BaseObject prev = GetObject(obj.CellPos);

		// 처음 신청했으면 해당 CellPos의 오브젝트가 본인이 아닐 수도 있음
		if (prev != obj)	// 이전 오브젝트 있으면 return
			return false;

		_cells[obj.CellPos] = null;
		return true;
	}

	public bool AddObject(BaseObject obj, Vector3Int cellPos)
	{
		if (CanGo(cellPos) == false)	// 먼저 갈 수 있는지 확인
		{
			Debug.LogWarning($"AddObject Failed");
			return false;
		}

		BaseObject prev = GetObject(cellPos);
		if (prev != null)	// 동일한 좌표에 다른 오브젝트 있으면 return
		{
			Debug.LogWarning($"AddObject Failed");	// 무조건 한칸에 하나의 오브젝트
			return false;
		}

		_cells[cellPos] = obj;
		return true;
	}

	public bool CanGo(Vector3 worldPos, bool ignoreObjects = false, bool ignoreSemiWall = false)
	{
		return CanGo(World2Cell(worldPos), ignoreObjects, ignoreSemiWall);
	}
	/// <summary>
	/// 그 위치에 갈 수 있는지
	/// </summary>							// 오브젝트 허락할지			// 카메라만 갈 수 있는지
	public bool CanGo(Vector3Int cellPos, bool ignoreObjects = false, bool ignoreSemiWall = false)
	{
		if (cellPos.x < MinX || cellPos.x > MaxX)
			return false;
		if (cellPos.y < MinY || cellPos.y > MaxY)
			return false;

		if (ignoreObjects == false)
		{
			BaseObject obj = GetObject(cellPos);
			if (obj != null)
				return false;
		}

		int x = cellPos.x - MinX;
		int y = MaxY - cellPos.y;
		ECellCollisionType type = _collision[x, y];
		if (type == ECellCollisionType.None)	// 갈 수 있는지 확인
			return true;

		if (ignoreSemiWall && type == ECellCollisionType.SemiWall)
			return true;

		return false;
	}

	public void ClearObjects()
	{
		_cells.Clear();
	}

	#endregion

	#region A* PathFinding
	public struct PQNode : IComparable<PQNode>
	{
		public int H; // Heuristic
		public Vector3Int CellPos;
		public int Depth;

		public int CompareTo(PQNode other)
		{
			if (H == other.H)
				return 0;
			return H < other.H ? 1 : -1;
		}
	}

	List<Vector3Int> _delta = new List<Vector3Int>()
	{
		new Vector3Int(0, 1, 0), // U
		new Vector3Int(1, 1, 0), // UR
		new Vector3Int(1, 0, 0), // R
		new Vector3Int(1, -1, 0), // DR
		new Vector3Int(0, -1, 0), // D
		new Vector3Int(-1, -1, 0), // LD
		new Vector3Int(-1, 0, 0), // L
		new Vector3Int(-1, 1, 0), // LU
	};

	//public List<Vector3Int> FindPath(Vector3Int startCellPos, Vector3Int destCellPos, int maxDepth = 10)
	//{
	//	// 지금까지 제일 좋은 후보 기록.
	//	Dictionary<Vector3Int, int> best = new Dictionary<Vector3Int, int>();
	//	// 경로 추적 용도.
	//	Dictionary<Vector3Int, Vector3Int> parent = new Dictionary<Vector3Int, Vector3Int>();

	//	// 현재 발견된 후보 중에서 가장 좋은 후보를 빠르게 뽑아오기 위한 도구.
	//	PriorityQueue<PQNode> pq = new PriorityQueue<PQNode>(); // OpenList

	//	Vector3Int pos = startCellPos;
	//	Vector3Int dest = destCellPos;

	//	// destCellPos에 도착 못하더라도 제일 가까운 애로.
	//	Vector3Int closestCellPos = startCellPos;
	//	int closestH = (dest - pos).sqrMagnitude;

	//	// 시작점 발견 (예약 진행)
	//	{
	//		int h = (dest - pos).sqrMagnitude;
	//		pq.Push(new PQNode() { H = h, CellPos = pos, Depth = 1 });
	//		parent[pos] = pos;
	//		best[pos] = h;
	//	}

	//	while (pq.Count > 0)
	//	{
	//		// 제일 좋은 후보를 찾는다
	//		PQNode node = pq.Pop();
	//		pos = node.CellPos;

	//		// 목적지 도착했으면 바로 종료.
	//		if (pos == dest)
	//			break;

	//		// 무한으로 깊이 들어가진 않음.
	//		if (node.Depth >= maxDepth)
	//			break;

	//		// 상하좌우 등 이동할 수 있는 좌표인지 확인해서 예약한다.
	//		foreach (Vector3Int delta in _delta)
	//		{
	//			Vector3Int next = pos + delta;

	//			// 갈 수 없는 장소면 스킵.
	//			if (CanGo(next) == false)
	//				continue;

	//			// 예약 진행
	//			int h = (dest - next).sqrMagnitude;

	//			// 더 좋은 후보 찾았는지
	//			if (best.ContainsKey(next) == false)
	//				best[next] = int.MaxValue;

	//			if (best[next] <= h)
	//				continue;

	//			best[next] = h;

	//			pq.Push(new PQNode() { H = h, CellPos = next, Depth = node.Depth + 1 });
	//			parent[next] = pos;

	//			// 목적지까지는 못 가더라도, 그나마 제일 좋았던 후보 기억.
	//			if (closestH > h)
	//			{
	//				closestH = h;
	//				closestCellPos = next;
	//			}
	//		}
	//	}

	//	// 제일 가까운 애라도 찾음.
	//	if (parent.ContainsKey(dest) == false)
	//		return CalcCellPathFromParent(parent, closestCellPos);

	//	return CalcCellPathFromParent(parent, dest);
	//}

	//List<Vector3Int> CalcCellPathFromParent(Dictionary<Vector3Int, Vector3Int> parent, Vector3Int dest)
	//{
	//	List<Vector3Int> cells = new List<Vector3Int>();

	//	if (parent.ContainsKey(dest) == false)
	//		return cells;

	//	Vector3Int now = dest;

	//	while (parent[now] != now)
	//	{
	//		cells.Add(now);
	//		now = parent[now];
	//	}

	//	cells.Add(now);
	//	cells.Reverse();

	//	return cells;
	//}

	#endregion
}
