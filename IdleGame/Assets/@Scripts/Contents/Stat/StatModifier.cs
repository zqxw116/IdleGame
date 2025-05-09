using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class StatModifier
{
	public readonly float Value;
	public readonly EStatModType Type;
	public readonly int Order; // 순서
	public readonly object Source;

	public StatModifier(float value, EStatModType type, int order, object source)
	{
		Value = value;
		Type = type;
		Order = order;
		Source = source;
	}

	public StatModifier(float value, EStatModType type) : this(value, type, (int)type, null) { }

	public StatModifier(float value, EStatModType type, int order) : this(value, type, order, null) { }

	public StatModifier(float value, EStatModType type, object source) : this(value, type, (int)type, source) { }
}