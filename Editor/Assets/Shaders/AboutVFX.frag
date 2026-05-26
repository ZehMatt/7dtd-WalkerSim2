precision highp float;

uniform vec2  u_resolution;
uniform float u_time;
uniform float u_bass;
uniform float u_energy;
uniform float u_perc;
uniform float u_lead;
uniform int   u_chord;
uniform float u_camZ;
uniform float u_phase;
uniform float u_flash;

#define MAT_GROUND 1.0
#define MAT_WALKER 2.0
#define MAT_POLE   3.0
#define MAT_LAMP   4.0

float sdCapsule(vec3 p, vec3 a, vec3 b, float r) {
    vec3 pa = p - a, ba = b - a;
    float h = clamp(dot(pa, ba) / dot(ba, ba), 0.0, 1.0);
    return length(pa - ba * h) - r;
}

float sdSphere(vec3 p, float r) { return length(p) - r; }

float sdCylY(vec3 p, float h, float r) {
    vec2 d = vec2(length(p.xz) - r, abs(p.y) - h);
    return min(max(d.x, d.y), 0.0) + length(max(d, 0.0));
}

float sminK(float a, float b, float k) {
    float h = clamp(0.5 + 0.5 * (b - a) / k, 0.0, 1.0);
    return mix(b, a, h) - k * h * (1.0 - h);
}

float vhash(vec2 p) {
    return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453);
}

float vnoise(vec2 p) {
    vec2 i = floor(p);
    vec2 f = fract(p);
    vec2 u = f * f * (3.0 - 2.0 * f);
    return mix(mix(vhash(i), vhash(i + vec2(1.0, 0.0)), u.x),
               mix(vhash(i + vec2(0.0, 1.0)), vhash(i + vec2(1.0, 1.0)), u.x), u.y);
}

float fbm2(vec2 p) {
    return vnoise(p) * 0.65 + vnoise(p * 2.3 + vec2(1.7, -0.9)) * 0.35;
}

float fbm3(vec2 p) {
    float a = vnoise(p);
    float b = vnoise(p * 2.07 + vec2(5.2, 1.3));
    float c = vnoise(p * 4.31 + vec2(-2.1, 3.8));
    return a * 0.55 + b * 0.30 + c * 0.15;
}

float walkerDE(vec3 p, float phase) {
    float bob = abs(sin(phase)) * 0.025;
    p.y -= bob;
    p.x -= sin(phase * 0.5) * 0.045;

    float sL = sin(phase) * 0.45;
    float sR = sin(phase + 3.14159) * 0.7;
    vec3 hipL = vec3(-0.11, 0.82, 0.0);
    vec3 hipR = vec3( 0.11, 0.82, 0.0);
    vec3 footL = hipL + vec3(0.0, -0.78 - sL * 0.02, sL * 0.14);
    vec3 footR = hipR + vec3(0.0, -0.78 - sR * 0.04, sR * 0.20);
    footL.y = max(footL.y, 0.03);
    footR.y = max(footR.y, 0.03);
    vec3 kneeL = (hipL + footL) * 0.5 + vec3(-0.04, 0.0, 0.02 * sL);
    vec3 kneeR = (hipR + footR) * 0.5 + vec3( 0.03, 0.0, 0.04 * sR);
    float legL = min(sdCapsule(p, hipL, kneeL, 0.07), sdCapsule(p, kneeL, footL, 0.055));
    float legR = min(sdCapsule(p, hipR, kneeR, 0.07), sdCapsule(p, kneeR, footR, 0.055));

    float torso = sdCapsule(p, vec3(0.0, 0.82, -0.04), vec3(-0.02, 1.34, 0.22), 0.13);
    float headTwist = sin(phase * 1.3) * 0.03;
    float head  = sdSphere(p - vec3(headTwist, 1.48, 0.36), 0.115);

    float sway = sin(phase * 0.7) * 0.05;
    vec3 shL = vec3(-0.17, 1.28, 0.20);
    vec3 shR = vec3( 0.17, 1.26, 0.20);
    vec3 elbL = vec3(-0.22, 1.06, 0.42);
    vec3 elbR = vec3( 0.20, 1.10, 0.40);
    vec3 handL = vec3(-0.20, 0.82 + sway, 0.62);
    vec3 handR = vec3( 0.22, 0.88 - sway, 0.58);
    float armL = min(sdCapsule(p, shL, elbL, 0.05), sdCapsule(p, elbL, handL, 0.04));
    float armR = min(sdCapsule(p, shR, elbR, 0.05), sdCapsule(p, elbR, handR, 0.04));

    float body = sminK(torso, head, 0.04);
    body = sminK(body, legL, 0.04);
    body = sminK(body, legR, 0.04);
    body = sminK(body, armL, 0.03);
    body = sminK(body, armR, 0.03);
    return body;
}

// Linear slope blended into a flat cap. Plain max() left a slope kink at
// the crossover that made hitP.z jump several meters across a narrow
// horizontal band, which broke road markings there.
float groundCurve(float worldZ, float camZ) {
    float dz = max(worldZ - camZ, 0.0);
    float slope = -0.055 * dz;
    float cap = -2.6;
    float t = smoothstep(-2.9, -2.3, slope);
    return mix(cap, slope, t);
}

float lampHeadDE(vec3 p, float camZ) {
    const float SP = 30.0;
    float zLocal = mod(p.z, SP) - SP * 0.5;
    float gY = groundCurve(p.z - zLocal, camZ);
    vec3 pL = vec3(p.x + 4.0, p.y - gY, zLocal);
    vec3 pR = vec3(p.x - 4.0, p.y - gY, zLocal);
    float lL = length(pL - vec3( 0.45, 3.08, 0.0)) - 0.14;
    float lR = length(pR - vec3(-0.45, 3.08, 0.0)) - 0.14;
    return min(lL, lR);
}

vec2 sceneDE(vec3 p, float camZ, float phase) {
    float gY = groundCurve(p.z, camZ);
    float d = p.y - gY;
    float mat = MAT_GROUND;

    {
        float zZ = camZ + 8.0;
        float gWY = groundCurve(zZ, camZ);
        float w = walkerDE(p - vec3(0.2, gWY, zZ), phase);
        if (w < d) { d = w; mat = MAT_WALKER; }
    }

    const float SPACING = 30.0;
    float zLocal = mod(p.z, SPACING) - SPACING * 0.5;
    float lampGY = groundCurve(p.z - zLocal, camZ);

    vec3 pL = vec3(p.x + 4.0, p.y - lampGY, zLocal);
    float poleL = sdCylY(pL - vec3(0.0, 1.6, 0.0), 1.6, 0.05);
    float barL  = sdCapsule(pL, vec3(0.0, 3.1, 0.0), vec3(0.45, 3.1, 0.0), 0.035);
    float lampL = sdSphere(pL - vec3(0.45, 3.08, 0.0), 0.14);
    float lmL = min(poleL, min(barL, lampL));
    if (lmL < d) {
        d = lmL;
        mat = (lampL <= poleL && lampL <= barL) ? MAT_LAMP : MAT_POLE;
    }

    vec3 pR = vec3(p.x - 4.0, p.y - lampGY, zLocal);
    float poleR = sdCylY(pR - vec3(0.0, 1.6, 0.0), 1.6, 0.05);
    float barR  = sdCapsule(pR, vec3(0.0, 3.1, 0.0), vec3(-0.45, 3.1, 0.0), 0.035);
    float lampR = sdSphere(pR - vec3(-0.45, 3.08, 0.0), 0.14);
    float lmR = min(poleR, min(barR, lampR));
    if (lmR < d) {
        d = lmR;
        mat = (lampR <= poleR && lampR <= barR) ? MAT_LAMP : MAT_POLE;
    }

    return vec2(d, mat);
}

vec3 calcNormal(vec3 p, float camZ, float phase) {
    vec2 e = vec2(0.002, 0.0);
    return normalize(vec3(
        sceneDE(p + e.xyy, camZ, phase).x - sceneDE(p - e.xyy, camZ, phase).x,
        sceneDE(p + e.yxy, camZ, phase).x - sceneDE(p - e.yxy, camZ, phase).x,
        sceneDE(p + e.yyx, camZ, phase).x - sceneDE(p - e.yyx, camZ, phase).x
    ));
}

// Maps a view ray to a 2D cloud-domain coord. Projecting onto the
// horizontal plane at a fixed altitude makes the cloud field appear to
// stretch toward the horizon naturally.
vec2 cloudDomain(vec3 rd, float t) {
    float y = max(rd.y, 0.02);
    vec2 q = rd.xz / y;
    q *= 0.18;
    q += vec2(t * 0.012, t * 0.006);
    return q;
}

float cloudCover(vec3 rd, float t) {
    if (rd.y < -0.05) return 0.0;
    vec2 q = cloudDomain(rd, t);
    float c = fbm3(q);
    c = smoothstep(0.30, 0.85, c);
    float horizonBoost = 1.0 - smoothstep(-0.05, 0.75, rd.y);
    c = mix(c, 1.0, horizonBoost * 0.55);
    return clamp(c, 0.0, 1.0);
}

vec3 roadColor(vec3 p, float dist) {
    float ax = abs(p.x);
    float aa = max(0.04, dist * 0.012);

    float distFadeFine = 1.0 - smoothstep(8.0, 22.0, dist);
    float distFadeMid  = 1.0 - smoothstep(20.0, 50.0, dist);
    float nFine   = vnoise(p.xz * 55.0) - 0.5;
    float nMid    = vnoise(p.xz * 14.0) - 0.5;
    float nCoarse = vnoise(p.xz *  3.2) - 0.5;
    float asphaltVar = nFine   * 0.018 * distFadeFine
                     + nMid    * 0.012 * distFadeMid
                     + nCoarse * 0.010;
    vec3 asphalt = vec3(0.08, 0.085, 0.095) + vec3(asphaltVar);

    float grassVar = (vnoise(p.xz * 5.5) - 0.5) * 0.04
                   + (vnoise(p.xz * 18.0) - 0.5) * 0.022 * distFadeMid;
    vec3 grass = vec3(0.045, 0.055, 0.04) + vec3(grassVar * 0.7, grassVar, grassVar * 0.5);

    vec3 line = vec3(0.78, 0.78, 0.82);
    vec3 dash = vec3(0.88, 0.72, 0.18);

    float offRoad = smoothstep(4.0 - aa, 4.0 + aa, ax);
    float edgeMask = 1.0 - smoothstep(0.06, 0.06 + aa, abs(ax - 3.5));

    float centerBand = 1.0 - smoothstep(0.10, 0.10 + aa, ax);
    float segZ = mod(p.z, 3.5);
    float dashOn = smoothstep(0.0, aa, segZ) * (1.0 - smoothstep(2.0, 2.0 + aa, segZ));
    float centerMask = centerBand * dashOn;

    vec3 col = mix(asphalt, grass, offRoad);
    col = mix(col, line, edgeMask * (1.0 - offRoad));
    col = mix(col, dash, centerMask);
    return col;
}

float puddleMask(vec3 p) {
    float ax = abs(p.x);
    if (ax > 3.95) return 0.0;
    float n = fbm2(p.xz * 0.55);
    float m = smoothstep(0.52, 0.68, n);
    float edge = 1.0 - smoothstep(3.4, 3.9, ax);
    return m * edge;
}

float puddleRipples(vec3 p, float t) {
    vec2 cell = floor(p.xz * 1.5);
    vec2 f = fract(p.xz * 1.5) - 0.5;
    float h = vhash(cell);
    float period = 0.7 + h * 1.6;
    float phase = fract(t / period + h);
    float r = phase * 0.45;
    float d = length(f * vec2(1.0, 1.0));
    float ring = smoothstep(0.035, 0.0, abs(d - r));
    return ring * (1.0 - phase);
}

float cityMask(vec3 rd) {
    if (rd.y <= -0.005 || rd.y >= 0.40 || abs(rd.x) >= 0.50) return 0.0;
    float xs = rd.x * 18.0;
    float h1 = vhash(vec2(floor(xs), 11.0));
    float h2 = vhash(vec2(floor(xs * 2.3), 19.0));
    float h3 = vhash(vec2(floor(xs * 5.1), 31.0));
    float centerBias = 1.0 - pow(clamp(abs(rd.x) / 0.50, 0.0, 1.0), 1.4);
    float buildH = (0.028 + h1 * 0.075 + h2 * 0.028 + h3 * 0.009)
                  * (0.30 + centerBias * 1.3);
    if (rd.y >= buildH) return 0.0;
    float edgeFade = smoothstep(0.50, 0.25, abs(rd.x));
    float vertFade = smoothstep(-0.005, buildH * 0.65, rd.y);
    return vertFade * edgeFade;
}

vec3 bloodMoon(vec3 rd, float cover) {
    vec3 moonDir = normalize(vec3(0.16, 0.22, 1.0));
    float cosAng = dot(rd, moonDir);
    if (cosAng <= 0.90) return vec3(0.0);

    float ang = acos(clamp(cosAng, -1.0, 1.0));
    float body = smoothstep(0.135, 0.120, ang);
    float halo = smoothstep(0.26, 0.12, ang) * (1.0 - body);

    vec3 localX = normalize(cross(moonDir, vec3(0.0, 1.0, 0.0)));
    vec3 localY = cross(localX, moonDir);
    vec2 mp = vec2(dot(rd, localX), dot(rd, localY));
    float mottle = fbm2(mp * 65.0) * 0.18 + fbm2(mp * 22.0) * 0.10;
    float limb = 1.0 - smoothstep(0.090, 0.130, ang) * 0.18;
    float shade = clamp(0.88 + mottle - 0.20, 0.78, 1.02) * limb;

    vec3 col = vec3(0.52, 0.04, 0.02) * body * shade
             + vec3(0.28, 0.025, 0.012) * halo * 0.85;
    return col * (1.0 - cover * 0.55);
}

vec3 skyColor(vec3 rd, float t, float lead, float flash) {
    vec3 base = vec3(0.012, 0.014, 0.022);

    float cover = cloudCover(rd, t);

    vec3 moonDir = normalize(vec3(0.16, 0.22, 1.0));
    float cosAng = dot(rd, moonDir);
    vec3 moonGlow = vec3(0.0);
    if (cosAng > 0.80) {
        float ang = acos(clamp(cosAng, -1.0, 1.0));
        float wash = smoothstep(0.50, 0.10, ang) * 0.18;
        moonGlow += vec3(0.30, 0.04, 0.018) * wash * (1.0 - cover * 0.5);
    }

    vec3 stars = vec3(0.0);
    if (rd.y > 0.0 && cover < 0.9) {
        vec2 sv = rd.xy * 95.0 + vec2(11.7, 2.9);
        vec2 cell = floor(sv);
        vec2 f = fract(sv) - 0.5;
        float h = vhash(cell);
        if (h > 0.97) {
            float tw = 0.85 + 0.10 * sin(h * 131.0 + t * 5.5);
            float pt = smoothstep(0.10, 0.015, length(f));
            vec3 sc = vec3(0.85, 0.90, 1.0);
            stars += sc * pt * tw * 0.55;
        }
        stars *= (1.0 - cover);
    }

    vec3 col = base + moonGlow + stars;

    {
        float altitude = max(rd.y, 0.0);
        float horizonBand = exp(-altitude * 2.6);
        float dome = horizonBand * (0.55 + 0.45 * cover);
        col += vec3(0.16, 0.075, 0.045) * dome;
    }
    if (rd.y > -0.005 && rd.y < 0.55 && abs(rd.x) < 0.55) {
        float altitude = max(rd.y, 0.0);
        float centerBias = 1.0 - pow(clamp(abs(rd.x) / 0.55, 0.0, 1.0), 1.4);
        float edgeFade = smoothstep(0.55, 0.25, abs(rd.x));
        float dome = exp(-altitude * 4.5) * edgeFade * (0.4 + centerBias * 0.7);
        col += vec3(0.22, 0.095, 0.055) * dome * (0.6 + cover * 0.9);
    }

    if (rd.y > -0.005 && rd.y < 0.40 && abs(rd.x) < 0.50) {
        float xs = rd.x * 18.0;
        float h1 = vhash(vec2(floor(xs), 11.0));
        float h2 = vhash(vec2(floor(xs * 2.3), 19.0));
        float h3 = vhash(vec2(floor(xs * 5.1), 31.0));
        float centerBias = 1.0 - pow(clamp(abs(rd.x) / 0.50, 0.0, 1.0), 1.4);
        float buildH = (0.028 + h1 * 0.075 + h2 * 0.028 + h3 * 0.009)
                      * (0.30 + centerBias * 1.3);
        float edgeFade = smoothstep(0.50, 0.25, abs(rd.x));
        if (rd.y < buildH) {
            float vertFade = smoothstep(-0.005, buildH * 0.65, rd.y);
            float darken = 0.95 * vertFade;
            col *= 1.0 - darken * edgeFade;
            col += vec3(0.010, 0.008, 0.006) * edgeFade * vertFade;
            col += vec3(0.30, 0.22, 0.18) * edgeFade * flash * 0.6 * vertFade;
        }
    }

    if (flash > 0.001) {
        float underside = smoothstep(0.0, 0.4, rd.y) * (1.0 - smoothstep(0.55, 0.85, rd.y));
        vec3 flashTint = vec3(0.85, 0.88, 1.0);
        col += flashTint * cover * underside * flash * 0.95;
        col += flashTint * flash * 0.08;
    }

    return col;
}

float rainLayer(vec2 uv, float t, vec2 scale, float speed, float threshold) {
    vec2 q = uv * scale;
    q.y += t * speed;
    q.x += sin(t * 0.3) * 0.15;
    vec2 cell = floor(q);
    vec2 f = fract(q) - vec2(0.5, 0.5);
    float h = vhash(cell);
    if (h < threshold) return 0.0;
    float streak = smoothstep(0.05, 0.0, abs(f.x))
                 * smoothstep(0.45, 0.0, abs(f.y))
                 * (1.0 - smoothstep(0.42, 0.50, abs(f.y)));
    return streak * (h - threshold) / (1.0 - threshold);
}

float rainAmount(vec2 uv, float t) {
    float a = rainLayer(uv * vec2(1.0, 0.42), t, vec2(28.0, 56.0), 38.0, 0.84) * 0.55;
    float b = rainLayer(uv * vec2(1.0, 0.42) + vec2(3.1, 7.7), t, vec2(46.0, 92.0), 62.0, 0.88) * 0.45;
    float c = rainLayer(uv * vec2(1.0, 0.42) + vec2(11.3, 1.7), t, vec2(80.0, 160.0), 105.0, 0.92) * 0.35;
    return a + b + c;
}

void main() {
    vec2 fc = gl_FragCoord.xy;
    vec2 uv = (fc - 0.5 * u_resolution) / u_resolution.y;
    uv.y += 0.18;
    float t = u_time;

    float bassP   = sqrt(clamp(u_bass,   0.0, 1.0));
    float percP   = sqrt(clamp(u_perc,   0.0, 1.0));
    float leadP   = sqrt(clamp(u_lead,   0.0, 1.0));
    float energyP = sqrt(clamp(u_energy, 0.0, 1.0));

    float camZ = u_camZ;
    float phase = u_phase;
    float flash = clamp(u_flash, 0.0, 1.2);

    float bobY = sin(phase * 2.0) * 0.025;
    float bobX = sin(phase) * 0.010;
    vec3 ro = vec3(bobX, 3.40 + bobY, camZ);
    float yaw = 0.14 * sin(t * 0.13) + 0.05 * sin(t * 0.047 + 1.7);
    float cy = cos(yaw), sy = sin(yaw);
    float pitch = -0.21;
    vec3 fwd = normalize(vec3(sy * cos(pitch), sin(pitch), cy * cos(pitch)));
    vec3 right = normalize(vec3(cy, 0.0, -sy));
    vec3 up = cross(fwd, right);
    vec3 rd = normalize(fwd + uv.x * right * 0.9 + uv.y * up * 0.9);

    float depth = 0.0;
    float mat = 0.0;
    float glow = 0.0;
    bool hit = false;
    vec3 hitP = vec3(0.0);
    const float MAX_DIST = 180.0;
    const float HIT_EPS = 0.003;
    for (int i = 0; i < 96; i++) {
        vec3 p = ro + rd * depth;
        vec2 r = sceneDE(p, camZ, phase);
        float d = r.x;
        float ld = lampHeadDE(p, camZ);
        float atten = exp(-depth * 0.08);
        glow += atten * 0.04 / (0.08 + ld * ld * 20.0);
        if (d < HIT_EPS) { hit = true; hitP = p; mat = r.y; break; }
        depth += d * 0.9;
        if (depth > MAX_DIST) break;
    }
    if (!hit && rd.y < -1e-5) {
        float tFlat = -(ro.y + 2.60) / rd.y;
        if (tFlat > 0.0) {
            hit = true;
            depth = tFlat;
            hitP = ro + rd * tFlat;
            mat = MAT_GROUND;
        }
    }

    vec3 col;
    if (!hit) {
        col = skyColor(rd, t, leadP, flash);
    } else {
        vec3 n = calcNormal(hitP, camZ, phase);
        vec3 lightDir = normalize(vec3(0.3, 0.7, -0.4));
        float diff = max(dot(n, lightDir), 0.0);

        if (mat < 1.5) {
            vec3 base = roadColor(hitP, depth) * (0.50 + 0.40 * diff);
            float pm = puddleMask(hitP);
            float grazing = pow(1.0 - clamp(-rd.y, 0.0, 1.0), 3.0);
            vec3 rRef = vec3(rd.x, -rd.y, rd.z);
            vec3 refSky = skyColor(rRef, t, leadP, flash);
            float reflectAmt = mix(0.12, 0.85, pm) * (0.35 + 0.65 * grazing);
            base = mix(base, refSky, reflectAmt);
            float ripple = puddleRipples(hitP, t) * pm;
            base += vec3(0.18, 0.20, 0.26) * ripple * (0.6 + flash * 1.8);
            col = base;
        } else if (mat < 2.5) {
            float rim = pow(1.0 - max(dot(n, -rd), 0.0), 2.5);
            col = vec3(0.015, 0.018, 0.025)
                + vec3(0.18, 0.22, 0.32) * rim * (0.8 + leadP * 0.6)
                + vec3(0.9, 0.55, 0.25) * rim * 0.18 * (1.0 + bassP);
            col += vec3(0.50, 0.55, 0.65) * rim * flash * 1.2;
        } else if (mat < 3.5) {
            col = vec3(0.055, 0.058, 0.072) * (0.4 + 0.8 * diff);
            col += vec3(0.30, 0.32, 0.40) * flash * 0.6;
        } else {
            float pulse = 0.9 + bassP * 3.0 + percP * 1.8;
            float lampAtten = exp(-depth * 0.018);
            col = vec3(0.78, 0.07, 0.03) * pulse * (0.40 + lampAtten * 0.65);
        }
    }

    bool isObject = hit && mat >= 1.5;
    float musicMask = isObject ? 1.0 : 0.0;

    vec3 haloCol = vec3(0.95, 0.18, 0.08);
    col += haloCol * glow * 0.45 * (1.0 + bassP * 1.5 * musicMask + percP * 1.2 * musicMask);

    vec3 moonDirShade = normalize(vec3(0.16, 0.22, 1.0));
    float moonAng = acos(clamp(dot(rd, moonDirShade), -1.0, 1.0));
    float moonScatter = exp(-moonAng * moonAng * 5.5);

    vec2 mistUV1 = uv * vec2(2.8, 1.6) + vec2(t * 0.07, t * 0.015);
    vec2 mistUV2 = uv * vec2(1.4, 0.9) + vec2(-t * 0.045, t * 0.03);
    float mist = fbm2(mistUV1) * 0.6 + fbm2(mistUV2) * 0.4;
    float mistBand = exp(-abs(rd.y - 0.05) * 3.2);
    float density = 1.0 + (mist * 2.0 - 1.0) * 0.30 * mistBand;
    float skyDist = 380.0 / (1.0 + max(rd.y, 0.0) * 9.0);
    float effDist = hit ? min(depth, skyDist) : skyDist;
    float extinction = exp(-effDist * 0.034 * density);

    float cover = cloudCover(rd, t);
    vec3 inscatter = vec3(0.030, 0.028, 0.034)
                   + vec3(0.95, 0.30, 0.13) * moonScatter * 0.22 * (1.0 - cover * 0.85)
                   + vec3(0.22, 0.10, 0.06) * exp(-max(rd.y, -0.05) * 3.0) * (0.5 + cover * 0.7);
    inscatter += vec3(0.55, 0.62, 0.78) * flash * (0.4 + cover * 0.6);
    inscatter *= 0.7 + mist * 0.6;

    if (hit) {
        col = col * extinction + inscatter * (1.0 - extinction);
    } else {
        float bandJitter = (fbm2(rd.xz * 1.6 + vec2(t * 0.04, 0.0)) - 0.5) * 0.18;
        float altFade = smoothstep(1.50, -0.30, rd.y + bandJitter);
        float floorHaze = 0.08;
        float hazeMask = mix(floorHaze, 0.82, altFade) * (0.55 + 0.45 * cover);
        col = mix(col, inscatter, clamp(hazeMask, 0.0, 0.85));
        float occl = 1.0 - cityMask(rd) * 0.96;
        col += bloodMoon(rd, cover) * occl;
    }

    const float WS_SLOT_DUR = 5.0;
    const float WS_WALK_SPEED = 1.1;
    float wsSlot = floor(t / WS_SLOT_DUR);
    float wsT = fract(t / WS_SLOT_DUR);
    float wh1 = fract(sin(wsSlot * 127.31) * 43758.5453);
    float wh2 = fract(sin(wsSlot * 311.71) * 43758.5453);
    float wh3 = fract(sin(wsSlot * 911.19) * 43758.5453);
    if (wh3 > 0.35) {
        float side = wh1 < 0.5 ? -1.0 : 1.0;
        float slotStartZ = camZ - wsT * WS_SLOT_DUR * WS_WALK_SPEED;
        vec3 wsWorld = vec3(
            side * (5.5 + wh1 * 8.0),
            0.0,
            slotStartZ + 38.0 + wh2 * 22.0);

        vec3 V = wsWorld - ro;
        float vF = dot(V, fwd);
        if (vF > 1.0 && (!hit || depth > vF)) {
            vec2 silCenter = vec2(dot(V, right), dot(V, up)) / (vF * 0.9);
            float silH = 1.8 / (vF * 0.9);
            float silW = 0.5 / (vF * 0.9);
            vec2 sLocal = (uv - silCenter) / vec2(silW, silH);
            float body = max(abs(sLocal.x) * 1.6 - 0.5, abs(sLocal.y - 0.45) * 2.4 - 0.95);
            float head = length((sLocal - vec2(0.0, 0.95)) * vec2(1.6, 1.2)) - 0.18;
            float silMask = smoothstep(0.04, -0.02, min(body, head));

            float fadeIn  = smoothstep(0.10, 0.30, wsT);
            float fadeOut = 1.0 - smoothstep(0.70, 0.90, wsT);
            float vis = fadeIn * fadeOut;
            float reveal = 0.75 + flash * 1.5;
            col = mix(col, vec3(0.008, 0.010, 0.016), silMask * vis * 0.95 * reveal);

            float eyeSep = 0.10;
            float eyeRad = 0.04;
            float eL = length(sLocal - vec2(-eyeSep, 0.95));
            float eR = length(sLocal - vec2( eyeSep, 0.95));
            vec3 eyeCol = vec3(1.0, 0.25, 0.08);
            col += eyeCol * smoothstep(eyeRad, eyeRad * 0.4, eL) * vis;
            col += eyeCol * smoothstep(eyeRad, eyeRad * 0.4, eR) * vis;
        }
    }

    float rain = rainAmount(uv, t);
    vec3 rainTint = vec3(0.55, 0.62, 0.78);
    col = mix(col, col + rainTint * 0.35, rain);
    col += rainTint * rain * (0.10 + flash * 0.8);

    col *= 1.0 + (percP * 0.55 + bassP * 0.25 + energyP * 0.15) * musicMask;
    col *= clamp(1.0 - dot(uv, uv) * 0.35, 0.0, 1.0);
    col = col / (1.0 + col);

    gl_FragColor = vec4(col, 1.0);
}
