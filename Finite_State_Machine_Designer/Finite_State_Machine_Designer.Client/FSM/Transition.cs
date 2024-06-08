﻿using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Finite_State_Machine_Designer.Client.FSM
{
    public class Transition
    {
        public Transition() { }

        public Transition(FiniteState fromState, FiniteState toState)
        {
            _toState = toState;
            _fromState = fromState;
        }

        public long Id { get; set; }

        private FiniteState _toState = new() { IsDrawable = false };
        private FiniteState _fromState = new() { IsDrawable = false };

        /// <summary>
        /// <para>List of states connected by this transition.</para>
        /// Maximum number is <b>2</b>.
        /// </summary>
        [JsonIgnore]
        public List<FiniteState> States
        {
            get => [_fromState, _toState];
            set
            {
                List<FiniteState> states = value.Where(x => x is not null).Take(2).ToList();
                if (states.Count > 0)
                    _fromState = states[0];
                if (states.Count > 1)
                    _toState = states[1];
                else
                    _toState = _fromState;
            }
        }

        [NotMapped]
        public FiniteState FromState
        {
            get => _fromState;
            set => _fromState = value;
        }

        [NotMapped]
        public FiniteState ToState
        {
            get => _toState;
            set => _toState = value;
        }

        /// <summary>
        /// It's the <see cref="CanvasCoordinate"/> that the transition originates from the edge
        /// of the state's <see cref="FiniteState.Radius"/>.
        /// <para>Note: If the <see cref="FromState"/> is <see langword="null"/>
        /// then it will return coordinates (0, 0)</para>
        /// </summary>
        public CanvasCoordinate FromCoord
        {
            get
            {
                double x;
                double y;
                if (!IsCurved)
                {
                    float radius = 1;
                    if (_fromState.Radius > 0)
                        radius = _fromState.Radius;
                    double angle = Angle;
                    x = _fromState.Coordinate.X + (Math.Cos(angle) * radius);
                    y = _fromState.Coordinate.Y + (Math.Sin(angle) * radius);
                }
                else
                {
                    double fromAngle = FromAngle;
                    x = CenterArc.X + (_radius * Math.Cos(fromAngle));
                    y = CenterArc.Y + (_radius * Math.Sin(fromAngle));
                }
                return new(x, y);
            }
        }

        /// <summary>
        /// It's the <see cref="CanvasCoordinate"/> that the transition goes to the edge
        /// of the state's <see cref="FiniteState.Radius"/>.
        /// <para>Note: If the <see cref="ToState"/> is <see langword="null"/>
        /// then it will return coordinates (0, 0)</para>
        /// </summary>
        public CanvasCoordinate ToCoord
        {
            get
            {
                double x;
                double y;
                if (!IsCurved)
                {
                    float radius = 1;
                    if (_toState.Radius > 0)
                        radius = _toState.Radius;
                    x = (Math.Cos(Angle + Math.PI) * radius) + _toState.Coordinate.X;
                    y = (Math.Sin(Angle + Math.PI) * radius) + _toState.Coordinate.Y;
                }
                else
                {
                    double toAngle = ToAngle;
                    x = CenterArc.X + (_radius * Math.Cos(toAngle));
                    y = CenterArc.Y + (_radius * Math.Sin(toAngle));
                }
                return new(x, y);
            }
        }

        /// <summary>
        /// Uses one of the formula of a
        /// <a href="https://en.wikipedia.org/wiki/Circular_segment#Radius_and_central_angle">circle segment</a>
        /// to get the angle from positive x axis.
        /// </summary>
        public double FromAngle
        {
            get
            {
                if (IsCurved)
                {
                    if (_fromState != _toState)
                    {
                        CanvasCoordinate fromCoord = _fromState.Coordinate;
                        double fromAngle = Math.Atan2(fromCoord.Y - _centerArc.Y, fromCoord.X - _centerArc.X);
                        /// 2 * Math.Asin(_fromState.Radius / (_radius * 2)) ≈ _fromState.Radius / _radius 
                        /// ≈ angle of the segment by around 3 sig figures
                        /// This is due to numbers less than 10^-3 stop making noticable differences on the canvas.
                        return fromAngle + ((IsReversed ? -1 : 1) * (_fromState.Radius / _radius));
                    }
                    else
                        return _selfFromAngle;
                }
                return Angle;
            }
        }

        /// <summary>
        /// Angle from the state when the transitions links the same finite state.
        /// </summary>
        private double _selfFromAngle;

        /// <summary>
        /// Uses one of the formula of a
        /// <a href="https://en.wikipedia.org/wiki/Circular_segment#Radius_and_central_angle">circle segment</a>
        /// to get the angle from positive x axis.
        /// </summary>
        public double ToAngle
        {
            get
            {
                if (IsCurved)
                {
                    if (_fromState != _toState)
                    {
                        CanvasCoordinate toCoord = _toState.Coordinate;
                        double toAngle = Math.Atan2(toCoord.Y - _centerArc.Y, toCoord.X - _centerArc.X);
                        /// 2 * Math.Asin(_toState.Radius / (_radius * 2)) ≈ _toState.Radius / _radius 
                        /// ≈ angle of the segment by around 3 sig figures
                        /// This is due to numbers less than 10^-3 stop making noticable differences on the canvas.
                        return toAngle - ((IsReversed ? -1 : 1) * (_toState.Radius / _radius));
                    }
                    else
                        return _selfToAngle;
                }
                return Angle;
            }
        }

        /// <summary>
        /// Angle to the state when the transitions links the same finite state.
        /// </summary>
        private double _selfToAngle;

        /// <summary>
        /// Anti-clockwise angle between the coordinates of <see cref="FromState"/> and the <see cref="ToState"/>.
        /// <para>If a self transition it represents the anti-clockwise angle from state to <see cref="CenterArc"/></para>
        /// </summary>
        public double Angle
        {
            get
            {
                if (_fromState != _toState)
                {
                    CanvasCoordinate dCoord = _toState.Coordinate - _fromState.Coordinate;
                    return Math.Atan2(dCoord.Y, dCoord.X);
                }
                return _selfAngle;
            }
        }

        private double _selfAngle;
        /// <summary>
        /// Sets the offset of the transition linking back to a state
        /// </summary>
        public double SelfAngle
        {
            get => _selfAngle;
            set
            {
                _selfAngle = value;
                double angleOffset = Math.PI * 0.8;
                _selfFromAngle = _selfAngle - angleOffset;
                _selfToAngle = _selfAngle + angleOffset;
            }
        }

        private CanvasCoordinate _centerArc;

        public CanvasCoordinate CenterArc
        {
            get
            {
                if (_fromState == _toState)
                {
                    double circleX = _fromState.Coordinate.X + (1.5 * _fromState.Radius * Math.Cos(_selfAngle));
                    double circleY = _fromState.Coordinate.Y + (1.5 * _fromState.Radius * Math.Sin(_selfAngle));

                    return new CanvasCoordinate(circleX, circleY);
                }
                if (IsCurved)
                    return _centerArc;
                return default;
            }
            set => _centerArc = value;
        }

        public CanvasCoordinate Anchor
        {
            get
            {
                CanvasCoordinate dCoord = new(_toState.Coordinate.X - _fromState.Coordinate.X,
                    _toState.Coordinate.Y - _fromState.Coordinate.Y);
                return new(_fromState.Coordinate.X + (dCoord.X * _parallelAxis) - (dCoord.Y * _perpendicularAxis),
                    _fromState.Coordinate.Y + (dCoord.Y * _parallelAxis) + (dCoord.X * _perpendicularAxis));
            }
        }

        public double ParallelAxis
        {
            get => _parallelAxis;
            set => _parallelAxis = value;
        }

        private double _parallelAxis;

        public double PerpendicularAxis
        {
            get => _perpendicularAxis;
            set => _perpendicularAxis = value;
        }

        private double _perpendicularAxis;

        public double MinPerpendicularDistance
        {
            get => _minPerpendicularDistance;
            set => _minPerpendicularDistance = value;
        }

        private double _minPerpendicularDistance = 0.02;

        public bool IsReversed { get; set; }

        private double _radius;

        public double Radius
        {
            get
            {
                if (_fromState != _toState)
                    return _radius;
                return 0.75 * _fromState.Radius;
            }
            set => _radius = value;
        }

        public bool IsCurved => Math.Abs(_perpendicularAxis) >= 0.02 || _fromState == _toState;

        private string _text = string.Empty;

        public string Text
        {
            get => _text;
            set => _text = value;
        }

        public override int GetHashCode() => HashCode.Combine(
            FromState, FromAngle, ToAngle, MinPerpendicularDistance * ParallelAxis, CenterArc * Anchor, Radius, IsReversed, ToState
        );

        public override string ToString()
        {
            string text = $"( {_fromState} -> {_toState}, Text: '{_text}', ";

            if (!IsCurved)
                text += $"Angle: {Angle} )";
            else
                text += $"From Angle: {FromAngle}, To Angle: {ToAngle}, Center Point: {_centerArc}, Radius: {_radius}, Reversed: {IsReversed} )";

            return text;
        }
    }
}