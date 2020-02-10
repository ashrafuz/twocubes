#ifndef SDF_2D
#define SDF_2D

half circle(half2 _samplePosition, half _radius){
    return length(_samplePosition) - _radius;
}

half rectanlge(half2 _samplePosition, half _width, half _height){
    half2 d = abs(_samplePosition) - 0.5*half2(_width, _height);
    half sdf = min(max(d.x, d.y), 0) + length(max(d, 0));
    return sdf;
}

half polygon(half2 _samplePosition, half _radius, half _sides, half _cornerRadius){
    return 0;
}

float ndot(float2 a, float2 b ) { return a.x*b.x - a.y*b.y; }
float sdRhombus( in float2 p, in float2 b ) {
    float2 q = abs(p);
    float h = clamp((-2.0*ndot(q,b)+ndot(b,b))/dot(b,b),-1.0,1.0);
    float d = length( q - 0.5*b*float2(1.0-h,1.0+h) );
    return d * sign( q.x*b.y + q.y*b.x - b.x*b.y );
}

float sdTriangleIsosceles( in float2 p, in float2 q )
{
    p.x = abs(p.x);
    float2 a = p - q*clamp( dot(p,q)/dot(q,q), 0.0, 1.0 );
    float2 b = p - q*float2( clamp( p.x/q.x, 0.0, 1.0 ), 1.0 );
    float s = -sign( q.y );
    float2 d = min( float2( dot(a,a), s*(p.x*q.y-p.y*q.x) ),
                  float2( dot(b,b), s*(p.y-q.y)  ));
    return -sqrt(d.x)*sign(d.y);
}


float2 rotate(float2 samplePosition, float rotation){
    const float PI = 3.14159;
    float angle = rotation * PI * 2 * -1;
    float sine, cosine;
    sincos(angle, sine, cosine);
    return float2(cosine * samplePosition.x + sine * samplePosition.y, cosine * samplePosition.y - sine * samplePosition.x);
}

half sampleSdf(half _sdf, half _offset){
    // half sdf = _sdf - _offset;
    // sdf = saturate(-sdf / fwidth(_offset));
    half sdf = saturate(-_sdf * _offset);
    return sdf;
}

half sampleSdfStrip(half _sdf, half _stripWidth, half _offset){
    
    half l = (_stripWidth+1/_offset)/2;
	return saturate((l-distance(-_sdf,l))*_offset);
}

half sdfUnion(half _a, half _b){
    return min(_a, _b);
}


half sdfIntersection(half _a, half _b){
    return max(_a, _b);
}

half sdfDifference(half _a, half _b)
{
    return max(_a, -_b);
}

half map(half value, half low1, half high1, half low2, half high2){
    return low2 + (value - low1) * (high2 - low2) / (high1 - low1);
}

#endif


/*
SDF Polygon
===================================
float2 f = UV - Position;
float theta = atan2(f.y, f.x);
float angle = 6.2831853071/Sides;
float SinSide, CosSide;
sincos(round(theta / angle) * angle, SinSide, CosSide);
float2 d = float2(SinSide, -CosSide);
float2 n = float2(CosSide, SinSide);
float t = dot(d, f);
float sideLength = Radius * tan(0.5*angle);
SDF = abs(t) < Radius * tan(0.5*angle) ? dot(f, n) - Radius : length(f - (Radius * n + d * clamp(dot(d, f), -sideLength, sideLength)));
SDF = SDF - CornerRadius;
*/