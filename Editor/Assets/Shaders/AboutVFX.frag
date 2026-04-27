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

float walkerDE(vec3 p, float phase) {
    float bob = abs(sin(phase)) * 0.025;
    p.y -= bob;
    p.x -= sin(phase * 0.5) * 0.045;

    // Asymmetric gait — left leg drags (smaller swing), right leg lifts.
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

    // Hunched torso leaning forward, narrower than a normal human's.
    float torso = sdCapsule(p, vec3(0.0, 0.82, -0.04), vec3(-0.02, 1.34, 0.22), 0.13);
    // Small head jutted forward and slumped, with a tiny twitch.
    float headTwist = sin(phase * 1.3) * 0.03;
    float head  = sdSphere(p - vec3(headTwist, 1.48, 0.36), 0.115);

    // Dangling arms — uneven angles, hands hang low and forward, thinner.
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

// Distance to the lamp-head spheres only — the march uses this (not the
// full scene DE) to accumulate halo glow, so grazing-the-road rays don't
// pick up bloom contributions from the ground surface.
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

vec3 roadColor(vec3 p, float dist) {
    float ax = abs(p.x);
    // Distance-scaled smoothstep widths so far pixels don't flicker as
    // sub-pixel features sweep past during camera motion.
    float aa = max(0.04, dist * 0.012);

    float distFadeFine = 1.0 - smoothstep(8.0, 22.0, dist);
    float distFadeMid  = 1.0 - smoothstep(20.0, 50.0, dist);
    float nFine   = vnoise(p.xz * 55.0) - 0.5;
    float nMid    = vnoise(p.xz * 14.0) - 0.5;
    float nCoarse = vnoise(p.xz *  3.2) - 0.5;
    float asphaltVar = nFine   * 0.018 * distFadeFine
                     + nMid    * 0.012 * distFadeMid
                     + nCoarse * 0.010;
    vec3 asphalt = vec3(0.13, 0.13, 0.14) + vec3(asphaltVar);

    float grassVar = (vnoise(p.xz * 5.5) - 0.5) * 0.04
                   + (vnoise(p.xz * 18.0) - 0.5) * 0.022 * distFadeMid;
    vec3 grass = vec3(0.06, 0.075, 0.05) + vec3(grassVar * 0.7, grassVar, grassVar * 0.5);

    vec3 line = vec3(0.85, 0.85, 0.88);
    vec3 dash = vec3(0.95, 0.78, 0.20);

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

vec3 skyColor(vec3 rd, float t, float chordShift, float lead) {
    vec3 col = vec3(0.0);

    vec3 moonDir = normalize(vec3(0.16, 0.22, 1.0));
    float cosAng = dot(rd, moonDir);
    if (cosAng > 0.93) {
        float ang = acos(clamp(cosAng, -1.0, 1.0));
        float body = smoothstep(0.130, 0.118, ang);
        float halo = smoothstep(0.22, 0.12, ang) * (1.0 - body);

        vec3 localX = normalize(cross(moonDir, vec3(0.0, 1.0, 0.0)));
        vec3 localY = cross(localX, moonDir);
        vec2 mp = vec2(dot(rd, localX), dot(rd, localY));
        // Hand-picked crater positions — a hash grid at this scale produced
        // visible square tiling on the moon surface.
        float shade = 1.0;
        shade -= smoothstep(0.018, 0.011, length(mp - vec2( 0.030,  0.045))) * 0.22;
        shade -= smoothstep(0.015, 0.009, length(mp - vec2(-0.055,  0.020))) * 0.18;
        shade -= smoothstep(0.012, 0.007, length(mp - vec2( 0.010, -0.048))) * 0.18;
        shade -= smoothstep(0.010, 0.006, length(mp - vec2(-0.030, -0.035))) * 0.14;
        shade -= smoothstep(0.009, 0.005, length(mp - vec2( 0.065, -0.015))) * 0.15;
        shade -= smoothstep(0.008, 0.004, length(mp - vec2(-0.070, -0.055))) * 0.12;
        shade -= smoothstep(0.011, 0.007, length(mp - vec2(-0.015,  0.070))) * 0.14;
        shade -= smoothstep(0.008, 0.005, length(mp - vec2( 0.055,  0.060))) * 0.10;
        shade -= smoothstep(0.007, 0.004, length(mp - vec2(-0.080,  0.045))) * 0.10;
        shade -= smoothstep(0.009, 0.005, length(mp - vec2( 0.085,  0.020))) * 0.10;
        shade -= smoothstep(0.007, 0.004, length(mp - vec2(-0.040,  0.055))) * 0.08;
        shade -= smoothstep(0.006, 0.003, length(mp - vec2( 0.040, -0.070))) * 0.08;
        shade -= smoothstep(0.006, 0.003, length(mp - vec2(-0.060, -0.010))) * 0.08;
        shade -= smoothstep(0.005, 0.003, length(mp - vec2( 0.020,  0.005))) * 0.06;
        shade -= smoothstep(0.005, 0.003, length(mp - vec2(-0.005, -0.015))) * 0.06;
        shade -= smoothstep(0.004, 0.002, length(mp - vec2( 0.075,  0.075))) * 0.06;
        shade -= smoothstep(0.005, 0.003, length(mp - vec2(-0.095, -0.020))) * 0.06;
        shade -= smoothstep(0.004, 0.002, length(mp - vec2( 0.095, -0.045))) * 0.05;
        shade = clamp(shade, 0.65, 1.0);

        col += vec3(0.85, 0.13, 0.06) * shade * body
             + vec3(0.30, 0.04, 0.02) * halo;
    }

    if (rd.y > -0.02) {
        vec3 stars = vec3(0.0);
        {
            vec2 sv = rd.xy * 45.0 + vec2(4.1, 7.3);
            vec2 cell = floor(sv);
            vec2 f = fract(sv) - 0.5;
            float h = vhash(cell);
            if (h > 0.985) {
                float tw = 0.92 + 0.05 * sin(h * 97.0 + t * 7.5)
                                + 0.03 * sin(h * 43.1 + t * 11.3);
                float core = smoothstep(0.13, 0.015, length(f));
                float temp = vhash(cell + 19.3);
                vec3 sc = temp > 0.5
                    ? mix(vec3(1.0, 1.0, 0.95), vec3(0.70, 0.82, 1.0), (temp - 0.5) * 2.0)
                    : mix(vec3(1.0, 0.50, 0.35), vec3(1.0, 1.0, 0.95), temp * 2.0);
                stars += sc * core * tw;
            }
        }
        {
            vec2 sv = rd.xy * 95.0 + vec2(11.7, 2.9);
            vec2 cell = floor(sv);
            vec2 f = fract(sv) - 0.5;
            float h = vhash(cell);
            if (h > 0.955) {
                float tw = 0.88 + 0.08 * sin(h * 131.0 + t * 5.5)
                                + 0.04 * sin(h * 71.7 + t * 8.9);
                float pt = smoothstep(0.11, 0.015, length(f));
                float temp = vhash(cell + 27.1);
                vec3 sc = temp > 0.5
                    ? mix(vec3(1.0, 0.95, 0.82), vec3(0.78, 0.88, 1.0), (temp - 0.5) * 2.0)
                    : mix(vec3(1.0, 0.65, 0.45), vec3(1.0, 0.95, 0.82), temp * 2.0);
                stars += sc * pt * tw * 0.65;
            }
        }
        {
            vec2 sv = rd.xy * 220.0 + vec2(6.2, 13.1);
            vec2 cell = floor(sv);
            vec2 f = fract(sv) - 0.5;
            float h = vhash(cell);
            if (h > 0.92) {
                float pt = smoothstep(0.09, 0.03, length(f));
                float temp = vhash(cell + 7.7);
                vec3 sc = mix(vec3(0.72, 0.78, 0.95), vec3(0.95, 0.90, 0.80), temp);
                stars += sc * pt * (0.28 + temp * 0.15);
            }
        }
        float moonFade = 1.0 - smoothstep(0.92, 0.99, cosAng);
        float horizonFade = smoothstep(-0.02, 0.12, rd.y);
        col += stars * moonFade * horizonFade;
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

        // Light-pollution dome — what the dark silhouettes are seen against
        // on an otherwise black sky.
        float altitude = max(rd.y, 0.0);
        float domeGlow = exp(-altitude * 6.0) * edgeFade * (0.4 + centerBias * 0.6);
        col += vec3(0.14, 0.065, 0.040) * domeGlow;

        if (rd.y < buildH) {
            col *= 1.0 - 0.95 * edgeFade;
            col += vec3(0.015, 0.011, 0.008) * edgeFade;
            // Faint outline at each building's vertical seam so adjacent
            // buildings stay visually separated even when their heights
            // happen to match. Two octaves matching the skyline hash so the
            // edges line up with the actual building boundaries.
            float ep1 = min(fract(xs), 1.0 - fract(xs));
            float ep2 = min(fract(xs * 2.3), 1.0 - fract(xs * 2.3));
            float outline = max(smoothstep(0.025, 0.0, ep1),
                                smoothstep(0.020, 0.0, ep2) * 0.6);
            col -= vec3(0.012, 0.009, 0.006) * outline * edgeFade;
        }
    }
    return col;
}

void main() {
    vec2 fc = gl_FragCoord.xy;
    vec2 uv = (fc - 0.5 * u_resolution) / u_resolution.y;
    // Pan the rendered view down so the scene clears the overlay header.
    uv.y += 0.18;
    float t = u_time;

    float bassP   = sqrt(clamp(u_bass,   0.0, 1.0));
    float percP   = sqrt(clamp(u_perc,   0.0, 1.0));
    float leadP   = sqrt(clamp(u_lead,   0.0, 1.0));
    float energyP = sqrt(clamp(u_energy, 0.0, 1.0));
    float chordShift = float(u_chord) * 0.11;

    float camZ = u_camZ;
    float phase = u_phase;

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
        // Halo accumulates from lamp-head distance only; using the scene
        // distance lets grazing-ground rays bloom into a horizon band.
        float ld = lampHeadDE(p, camZ);
        float atten = exp(-depth * 0.08);
        glow += atten * 0.04 / (0.08 + ld * ld * 20.0);
        if (d < HIT_EPS) { hit = true; hitP = p; mat = r.y; break; }
        depth += d * 0.9;
        if (depth > MAX_DIST) break;
    }
    // Analytical fallback for rays that exceed MAX_DIST: any down-angled
    // ray would mathematically still hit the flat far-ground plane. Without
    // this, the march cutoff produces a sharp horizontal seam where rays
    // flip from ground hit to sky.
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
        col = skyColor(rd, t, chordShift, leadP);
    } else {
        vec3 n = calcNormal(hitP, camZ, phase);
        vec3 lightDir = normalize(vec3(0.3, 0.7, -0.4));
        float diff = max(dot(n, lightDir), 0.0);

        if (mat < 1.5) {
            col = roadColor(hitP, depth) * (0.55 + 0.45 * diff);
        } else if (mat < 2.5) {
            float rim = pow(1.0 - max(dot(n, -rd), 0.0), 2.5);
            col = vec3(0.02, 0.02, 0.03)
                + vec3(0.15, 0.18, 0.28) * rim * (0.8 + leadP * 0.6)
                + vec3(0.9, 0.55, 0.25) * rim * 0.18 * (1.0 + bassP);
        } else if (mat < 3.5) {
            col = vec3(0.06, 0.06, 0.08) * (0.4 + 0.8 * diff);
        } else {
            float pulse = 0.9 + bassP * 3.0 + percP * 1.8;
            float lampAtten = exp(-depth * 0.018);
            col = vec3(0.75, 0.06, 0.03) * pulse * (0.35 + lampAtten * 0.65);
        }
    }

    // Music modulation skipped on road and sky so they stay constant.
    bool isObject = hit && mat >= 1.5;
    float musicMask = isObject ? 1.0 : 0.0;

    vec3 haloCol = vec3(0.90, 0.12, 0.05);
    col += haloCol * glow * 0.32 * (1.0 + bassP * 1.5 * musicMask + percP * 1.2 * musicMask);

    // Volumetric fog: extinction × background + inscatter from moon.
    vec3 moonDirShade = normalize(vec3(0.16, 0.22, 1.0));
    float moonAng = acos(clamp(dot(rd, moonDirShade), -1.0, 1.0));
    float moonScatter = exp(-moonAng * moonAng * 5.5);

    vec2 smokeUV1 = uv * vec2(2.8, 1.6) + vec2(t * 0.07, t * 0.015);
    vec2 smokeUV2 = uv * vec2(1.4, 0.9) + vec2(-t * 0.045, t * 0.03);
    float smoke = fbm2(smokeUV1) * 0.6 + fbm2(smokeUV2) * 0.4;
    float smokeBand = exp(-abs(rd.y - 0.02) * 4.0);
    float density = 1.0 + (smoke * 2.0 - 1.0) * 0.35 * smokeBand;

    float skyDist = 400.0 / (1.0 + max(rd.y, 0.0) * 9.0);
    float effDist = hit ? min(depth, skyDist) : skyDist;
    float extinction = exp(-effDist * 0.022 * density);

    vec3 inscatter = vec3(0.020, 0.012, 0.010) + vec3(1.00, 0.28, 0.12) * moonScatter * 0.35;
    inscatter *= 0.7 + smoke * 0.6;

    if (hit) {
        col = col * extinction + inscatter * (1.0 - extinction);
    }

    // Distant walker silhouette — anchored to a world position off the
    // road and in the fog, fades in and out over each slot.
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
            // Walker silhouette in normalized local space:
            //   x in [-0.5, 0.5], y in [0, 1] (feet → top of head).
            float body = max(abs(sLocal.x) * 1.6 - 0.5, abs(sLocal.y - 0.45) * 2.4 - 0.95);
            float head = length((sLocal - vec2(0.0, 0.95)) * vec2(1.6, 1.2)) - 0.18;
            float silMask = smoothstep(0.04, -0.02, min(body, head));

            float fadeIn  = smoothstep(0.10, 0.30, wsT);
            float fadeOut = 1.0 - smoothstep(0.70, 0.90, wsT);
            float vis = fadeIn * fadeOut;
            col = mix(col, vec3(0.008, 0.008, 0.012), silMask * vis * 0.95);

            // Tiny glowing eyes on the silhouette's head (sLocal y≈0.95).
            float eyeSep = 0.10;
            float eyeRad = 0.04;
            float eL = length(sLocal - vec2(-eyeSep, 0.95));
            float eR = length(sLocal - vec2( eyeSep, 0.95));
            vec3 eyeCol = vec3(1.0, 0.25, 0.08);
            col += eyeCol * smoothstep(eyeRad, eyeRad * 0.4, eL) * vis;
            col += eyeCol * smoothstep(eyeRad, eyeRad * 0.4, eR) * vis;
        }
    }

    col *= 1.0 + (percP * 0.55 + bassP * 0.25 + energyP * 0.15) * musicMask;
    col *= clamp(1.0 - dot(uv, uv) * 0.35, 0.0, 1.0);
    col = col / (1.0 + col);

    gl_FragColor = vec4(col, 1.0);
}
