(function () {
    'use strict';

    var modId = 'WalkerSim2';
    var apiBase = '/api/WalkerSim2';
    var permModule = 'webapi.WalkerSim2';
    var prefKey = 'walkersim2.colorByGroup';

    function hashHue(i) {
        return ((i * 137) % 360 + 360) % 360;
    }

    function circleVerts(lat, lng, radius, segments) {
        var pts = [];
        for (var i = 0; i < segments; i++) {
            var a = (i / segments) * Math.PI * 2;
            pts.push([lat + Math.cos(a) * radius, lng + Math.sin(a) * radius]);
        }
        return pts;
    }

    function loadPref() {
        try {
            var v = window.localStorage.getItem(prefKey);
            return v === null ? false : v === 'true';
        } catch (e) { return false; }
    }

    function savePref(v) {
        try { window.localStorage.setItem(prefKey, v ? 'true' : 'false'); } catch (e) { }
    }

    function MapComponent(props) {
        var React = props.React;
        var HTTP = props.HTTP;
        var useQuery = props.useQuery;
        var checkPermission = props.checkPermission;
        var LayerGroup = props.LayerGroup;
        var LayersControl = props.LayersControl;
        var HideBasedOnAuth = props.HideBasedOnAuth;
        var L = props.L;
        var map = props.map;
        var h = React.createElement;

        var canSee = checkPermission({ module: permModule, method: 'GET' });

        var staticQ = useQuery('walkersim-static', function () {
            return HTTP.get(apiBase + '/static');
        }, { enabled: canSee, staleTime: 5 * 60 * 1000 });

        var snapshotQ = useQuery('walkersim-snapshot', function () {
            return HTTP.get(apiBase + '/snapshot');
        }, { enabled: canSee, refetchInterval: 1000, staleTime: 0 });

        var stat = staticQ.data || null;
        var snap = snapshotQ.data || { a: [], s: [], p: [], e: [] };

        var colorByGroupState = React.useState(loadPref());
        var colorByGroup = colorByGroupState[0];
        var setColorByGroup = colorByGroupState[1];

        var canvasRenderer = React.useMemo(function () {
            return L.canvas({ padding: 0.5 });
        }, [L]);

        var citiesRef = React.useRef(null);
        var wanderingRef = React.useRef(null);
        var spawnedRef = React.useRef(null);
        var playersRef = React.useRef(null);
        var graphRef = React.useRef(null);
        var eventsRef = React.useRef(null);
        var snapDataRef = React.useRef({ a: [], s: [], p: [], e: [] });
        snapDataRef.current = snap;

        React.useEffect(function () {
            if (!map) return;
            var Ctl = L.Control.extend({
                onAdd: function () {
                    var div = L.DomUtil.create('div', 'leaflet-bar walkersim-control');
                    div.style.padding = '4px 8px';
                    div.style.background = '#fff';
                    div.style.color = '#222';
                    div.style.fontSize = '12px';
                    div.innerHTML =
                        '<label style="display:flex;align-items:center;gap:4px;cursor:pointer;">' +
                        '<input type="checkbox" id="walkersim-color-toggle"' + (colorByGroup ? ' checked' : '') + '>' +
                        'WalkerSim group colors</label>';
                    L.DomEvent.disableClickPropagation(div);
                    var input = div.querySelector('#walkersim-color-toggle');
                    input.addEventListener('change', function (e) {
                        var v = !!e.target.checked;
                        savePref(v);
                        setColorByGroup(v);
                    });
                    return div;
                }
            });
            var control = new Ctl({ position: 'topright' }).addTo(map);
            return function () { map.removeControl(control); };
        }, [map, L]);

        React.useEffect(function () {
            var lg = citiesRef.current;
            if (!lg || !stat || !stat.cities) return;
            lg.clearLayers();
            for (var i = 0; i < stat.cities.length; i++) {
                var c = stat.cities[i];
                var hue = hashHue(c.id);
                var halfX = c.bx / 2;
                var halfY = c.by / 2;
                var bounds = [[c.x - halfX, -(c.y - halfY)], [c.x + halfX, -(c.y + halfY)]];
                L.rectangle(bounds, {
                    renderer: canvasRenderer,
                    color: 'hsl(' + hue + ',75%,65%)',
                    fillColor: 'hsl(' + hue + ',75%,55%)',
                    fillOpacity: 0.25,
                    weight: 1,
                    interactive: false
                }).addTo(lg);
            }
        }, [stat, canvasRenderer, L]);

        React.useEffect(function () {
            var lg = wanderingRef.current;
            if (!lg || !stat) return;
            lg.clearLayers();
            var groupColors = stat.groupColors || [];
            for (var i = 0; i < snap.a.length; i += 3) {
                var color;
                if (colorByGroup) {
                    var g = snap.a[i + 2];
                    color = groupColors[g % (groupColors.length || 1)] || '#ffffff';
                } else {
                    color = '#ff0000';
                }
                L.circleMarker([snap.a[i], -snap.a[i + 1]], {
                    renderer: canvasRenderer,
                    radius: 1, color: color, fillColor: color, fillOpacity: 0.9, weight: 0,
                    interactive: false
                }).addTo(lg);
            }
        }, [snap, stat, colorByGroup, canvasRenderer, L]);

        React.useEffect(function () {
            var lg = spawnedRef.current;
            if (!lg || !stat) return;
            lg.clearLayers();
            var groupColors = stat.groupColors || [];
            for (var i = 0; i < snap.s.length; i += 3) {
                var color;
                if (colorByGroup) {
                    var g = snap.s[i + 2];
                    color = groupColors[g % (groupColors.length || 1)] || '#00ff00';
                } else {
                    color = '#00ff00';
                }
                L.circleMarker([snap.s[i], -snap.s[i + 1]], {
                    renderer: canvasRenderer,
                    radius: 2, color: color, fillColor: color, fillOpacity: 1.0, weight: 0,
                    interactive: false
                }).addTo(lg);
            }
        }, [snap, stat, colorByGroup, canvasRenderer, L]);

        React.useEffect(function () {
            var lg = playersRef.current;
            if (!lg) return;
            lg.clearLayers();
            var vr = (stat && stat.viewRadius) ? stat.viewRadius : 96;
            var bs = 12;
            var inner = Math.max(1, vr - bs);
            for (var i = 0; i < snap.p.length; i += 2) {
                var pos = [snap.p[i], -snap.p[i + 1]];
                var outerRing = circleVerts(pos[0], pos[1], vr, 64);
                var innerRing = circleVerts(pos[0], pos[1], inner, 64);

                L.polygon([outerRing, innerRing], {
                    renderer: canvasRenderer,
                    stroke: false,
                    fill: true, fillColor: '#00f000', fillOpacity: 0.2,
                    interactive: false
                }).addTo(lg);

                L.polygon(outerRing, {
                    renderer: canvasRenderer,
                    stroke: true, color: '#0000ff', weight: 1, opacity: 0.5,
                    fill: false,
                    interactive: false
                }).addTo(lg);

                L.polygon(innerRing, {
                    renderer: canvasRenderer,
                    stroke: true, color: '#ffff00', weight: 1, opacity: 0.5,
                    fill: false,
                    interactive: false
                }).addTo(lg);

                L.circleMarker(pos, {
                    renderer: canvasRenderer,
                    radius: 3, stroke: false, fillColor: '#ff00ff', fillOpacity: 1.0
                }).addTo(lg);
            }
        }, [snap, stat, canvasRenderer, L]);

        var graphPolylineRef = React.useRef(null);

        function computeEdgeWeight(m, L) {
            // Pixels per world unit at current zoom (CRS.Simple uses identity for x).
            var p1 = m.latLngToLayerPoint(L.latLng(0, 0));
            var p2 = m.latLngToLayerPoint(L.latLng(0, 1));
            var pxPerUnit = Math.abs(p2.x - p1.x);
            // Aim for ~0.8 world units of stroke (matches editor's scaleX*0.8).
            return Math.max(0.5, pxPerUnit * 0.8);
        }

        React.useEffect(function () {
            if (!map) return;
            function onZoom() {
                var pl = graphPolylineRef.current;
                if (pl) pl.setStyle({ weight: computeEdgeWeight(map, L) });
            }
            map.on('zoomend', onZoom);
            return function () { map.off('zoomend', onZoom); };
        }, [map, L]);

        React.useEffect(function () {
            var rafId = 0;
            var ringCount = 4;
            var ringSpacing = 24;
            var maxOffset = (ringCount - 1) * ringSpacing;

            function tick() {
                var lg = eventsRef.current;
                if (lg) {
                    lg.clearLayers();
                    var ev = snapDataRef.current && snapDataRef.current.e ? snapDataRef.current.e : [];
                    var t = (performance.now() % 2500) / 2500;
                    for (var i = 0; i < ev.length; i += 3) {
                        var ex = ev[i];
                        var ey = -ev[i + 1];
                        var er = ev[i + 2];
                        var total = er + maxOffset;
                        for (var r = 0; r < ringCount; r++) {
                            var offset = r * ringSpacing;
                            var animatedRadius = total * t - offset;
                            if (animatedRadius > er || animatedRadius < 1) continue;
                            L.polygon(circleVerts(ex, ey, animatedRadius, 32), {
                                renderer: canvasRenderer,
                                stroke: true, color: '#c88080', weight: 1, opacity: 0.5,
                                fill: false,
                                interactive: false
                            }).addTo(lg);
                        }
                    }
                }
                rafId = window.requestAnimationFrame(tick);
            }
            rafId = window.requestAnimationFrame(tick);
            return function () { window.cancelAnimationFrame(rafId); };
        }, [canvasRenderer, L]);

        React.useEffect(function () {
            var lg = graphRef.current;
            if (!lg || !stat || !stat.graph) return;
            lg.clearLayers();
            graphPolylineRef.current = null;
            var nodes = stat.graph.nodes;
            var edges = stat.graph.edges;
            var n = nodes.length / 3;

            var segments = [];
            for (var e = 0; e < edges.length; e += 2) {
                var a = edges[e];
                var b = edges[e + 1];
                if (a >= n || b >= n) continue;
                segments.push([[nodes[a * 3], -nodes[a * 3 + 1]], [nodes[b * 3], -nodes[b * 3 + 1]]]);
            }
            if (segments.length > 0) {
                graphPolylineRef.current = L.polyline(segments, {
                    renderer: canvasRenderer,
                    color: '#00c8ff',
                    opacity: 0.5,
                    weight: map ? computeEdgeWeight(map, L) : 1,
                    interactive: false
                }).addTo(lg);
            }

            for (var i = 0; i < n; i++) {
                var x = nodes[i * 3];
                var y = nodes[i * 3 + 1];
                var t = nodes[i * 3 + 2];
                var color;
                if (t === 1) color = '#ff5050';
                else if (t === 2) color = '#ffdc00';
                else color = '#00c8ff';
                L.circleMarker([x, -y], {
                    renderer: canvasRenderer,
                    radius: 2, stroke: false, fillColor: color, fillOpacity: 0.9,
                    interactive: false
                }).addTo(lg);
            }
        }, [stat, canvasRenderer, L]);

        if (!canSee || !stat || !stat.world) {
            return null;
        }

        return h(HideBasedOnAuth,
            { requiredPermission: { module: permModule, method: 'GET' } },
            h(LayersControl.Overlay, { name: 'WalkerSim Cities', checked: false },
                h(LayerGroup, { ref: citiesRef })),
            h(LayersControl.Overlay, { name: 'WalkerSim Road Graph', checked: false },
                h(LayerGroup, { ref: graphRef })),
            h(LayersControl.Overlay, { name: 'WalkerSim Wandering', checked: true },
                h(LayerGroup, { ref: wanderingRef })),
            h(LayersControl.Overlay, { name: 'WalkerSim Spawned', checked: true },
                h(LayerGroup, { ref: spawnedRef })),
            h(LayersControl.Overlay, { name: 'WalkerSim Players', checked: true },
                h(LayerGroup, { ref: playersRef })),
            h(LayersControl.Overlay, { name: 'WalkerSim Sound Events', checked: true },
                h(LayerGroup, { ref: eventsRef }))
        );
    }

    var WalkerSim = {
        about: 'Live view of the WalkerSim simulation: agent positions, cities, and players.',
        routes: {},
        settings: {},
        mapComponents: [MapComponent]
    };

    window[modId] = WalkerSim;
    window.dispatchEvent(new Event('mod:' + modId + ':ready'));
})();
