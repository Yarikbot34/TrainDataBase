
(function () {
    'use strict';

    const STATIONS_URL = '/api/v1/content/writedStations';
    const SAVE_URL = '/api/v1/map/uploadSchema';


    const NODE_WIDTH = 150;
    const NODE_HEIGHT = 64;
    const NODE_FILL = '#1e3a8a';
    const NODE_STROKE = '#16306e';

    const $ = (id) => document.getElementById(id);

    const els = {
        schemaName: $('schema-name'),
        schemaDescription: $('schema-description'),
        stationSelect: $('station-select'),
        addStationBtn: $('add-station-btn'),
        saveBtn: $('save-schema-btn'),
        clearBtn: $('clear-canvas-btn'),
        zoomIn: $('zoom-in-btn'),
        zoomOut: $('zoom-out-btn'),
        zoomFit: $('zoom-fit-btn'),
        zoomReset: $('zoom-reset-btn'),
        edgePanel: $('edge-panel'),
        edgeLabel: $('edge-panel-label'),
        edgeDelete: $('edge-delete-btn'),
        notification: $('map-notification'),
    };

    let graph = null;
    let allStations = [];
    const placedStations = new Set();  
    let activeEdge = null;              
    let selectedNode = null;           
    let notifyTimer = null;

    document.addEventListener('DOMContentLoaded', () => {
        if (!window.X6 || !window.X6.Graph) {
            notify('Не удалось загрузить библиотеку AntV X6', true);
            return;
        }
        initGraph();
        bindEvents();
        loadStations();
    });



    function initGraph() {
        const { Graph } = window.X6;

        Graph.registerNode('station-node', {
            inherit: 'rect',
            width: NODE_WIDTH,
            height: NODE_HEIGHT,
            attrs: {
                body: { fill: NODE_FILL, stroke: NODE_STROKE, strokeWidth: 1, rx: 3, ry: 3 },
                label: { fill: '#ffffff', fontSize: 13, fontWeight: 600 },
            },
            ports: {
                groups: {
                    top: makePortGroup('top'),
                    right: makePortGroup('right'),
                    bottom: makePortGroup('bottom'),
                    left: makePortGroup('left'),
                },
                items: [
                    { id: 'top', group: 'top' },
                    { id: 'right', group: 'right' },
                    { id: 'bottom', group: 'bottom' },
                    { id: 'left', group: 'left' },
                ],
            },
        }, true);

        Graph.registerEdge('load-edge', {
            inherit: 'edge',
            router: { name: 'manhattan', args: { step: 10, padding: 8 } },
            connector: { name: 'rounded', args: { radius: 4 } },
            attrs: {
                line: { stroke: '#00151E', strokeWidth: 2, sourceMarker: null, targetMarker: null },
            },
        }, true);

        graph = new Graph({
            container: $('graph-container'),
            autoResize: true,
            background: { color: '#f8faff' },
            grid: { visible: true, size: 12, type: 'dot', args: { color: '#c3cede', radius: 1.2 } },
            panning: { enabled: true },
            mousewheel: { enabled: true, minScale: 0.4, maxScale: 2.5 },
            highlighting: {
                magnetAvailable: {
                    name: 'stroke',
                    args: { attrs: { fill: '#eaf2ff', stroke: '#1769e0', strokeWidth: 2 } },
                },
                magnetAdsorbed: {
                    name: 'stroke',
                    args: { attrs: { fill: '#dceaff', stroke: '#0f55ba', strokeWidth: 3 } },
                },
            },
            connecting: {
                snap: { radius: 24 },
                allowBlank: false,
                allowLoop: false,
                allowMulti: false,
                allowNode: false,
                allowEdge: false,
                allowPort: true,
                highlight: true,
                createEdge() {
                    return graph.createEdge({ shape: 'load-edge', data: { load: 0 } });
                },
                validateConnection({ sourceView, targetView }) {
                    if (!sourceView || !targetView) return false;
                    return sourceView.cell.id !== targetView.cell.id;
                },
            },
        });

        
        graph.on('node:mouseleave', ({ node }) => node.removeTools());

        graph.on('node:click', ({ node }) => { selectNode(node); closeEdgePanel(); });
        graph.on('blank:click', () => { selectNode(null); closeEdgePanel(); });
        graph.on('edge:click', ({ edge, e }) => { selectNode(null); openEdgePanel(edge, e); });


        graph.on('node:removed', ({ node }) => {
            const label = node.getData()?.label;
            if (label) placedStations.delete(label);
            if (selectedNode && selectedNode.id === node.id) selectedNode = null;
            refreshStationOptions();
        });

        graph.on('edge:removed', ({ edge }) => {
            if (activeEdge && activeEdge.id === edge.id) closeEdgePanel();
        });
    }

    function makePortGroup(position) {
        return {
            position,
            attrs: {
                circle: { r: 5, magnet: true, stroke: '#1769e0', fill: '#ffffff', strokeWidth: 1.5 },
            },
        };
    }

    function selectNode(node) {
        if (selectedNode && (!node || selectedNode.id !== node.id)) {
            selectedNode.attr('body/stroke', NODE_STROKE);
            selectedNode.attr('body/strokeWidth', 1);
        }
        selectedNode = node || null;
        if (selectedNode) {
            selectedNode.attr('body/stroke', '#1769e0');
            selectedNode.attr('body/strokeWidth', 2);
        }
    }
    

    async function loadStations() {
        try {
            const res = await fetch(STATIONS_URL);
            if (!res.ok) throw new Error('HTTP ' + res.status);
            const data = await res.json();
            const list = Array.isArray(data) ? data : (data.items || data.stations || []);
            allStations = [...new Set(list.map((s) => String(s).trim()).filter(Boolean))]
                .sort((a, b) => a.localeCompare(b, 'ru'));
            refreshStationOptions();
        } catch (err) {
            notify('Не удалось загрузить список станций', true);
        }
    }


    function refreshStationOptions() {
        const available = allStations.filter((s) => !placedStations.has(s));
        els.stationSelect.innerHTML = '';

        if (!available.length) {
            const opt = document.createElement('option');
            opt.value = '';
            opt.textContent = '— все станции уже на карте —';
            opt.disabled = true;
            opt.selected = true;
            els.stationSelect.appendChild(opt);
            els.stationSelect.disabled = true;
            els.addStationBtn.disabled = true;
            return;
        }

        available.forEach((name) => {
            const opt = document.createElement('option');
            opt.value = name;
            opt.textContent = name;
            els.stationSelect.appendChild(opt);
        });
        els.stationSelect.disabled = false;
        els.addStationBtn.disabled = false;
    }

    function addStation() {
        const name = els.stationSelect.value;
        if (!name || placedStations.has(name)) return;

        const rect = graph.container.getBoundingClientRect();
        const center = graph.clientToLocal({
            x: rect.left + rect.width / 2,
            y: rect.top + rect.height / 2,
        });
        const shift = (graph.getNodes().length % 6) * 26;

        graph.addNode({
            shape: 'station-node',
            x: Math.round(center.x - NODE_WIDTH / 2 + shift),
            y: Math.round(center.y - NODE_HEIGHT / 2 + shift),
            data: { label: name },
            attrs: { label: { text: name } },
        });

        placedStations.add(name);
        refreshStationOptions();
    }
    

    function openEdgePanel(edge, mouseEvent) {
        activeEdge = edge;

        const s = getNodeLabel(edge.getSourceCellId());
        const t = getNodeLabel(edge.getTargetCellId());
        els.edgeLabel.textContent = s && t ? s + ' — ' + t : 'связь';

        const canvas = document.querySelector('.editor-canvas');
        const rect = canvas.getBoundingClientRect();
        let x = (mouseEvent?.clientX ?? rect.left + 40) - rect.left + 14;
        let y = (mouseEvent?.clientY ?? rect.top + 40) - rect.top + 14;
        x = Math.min(Math.max(8, x), rect.width - 330);
        y = Math.min(Math.max(8, y), rect.height - 80);

        els.edgePanel.style.left = x + 'px';
        els.edgePanel.style.top = y + 'px';
        els.edgePanel.hidden = false;
    }

    function closeEdgePanel() {
        activeEdge = null;
        els.edgePanel.hidden = true;
    }

    function getNodeLabel(id) {
        const cell = id ? graph.getCellById(id) : null;
        return cell?.getData()?.label ?? '';
    }


    function serializeCell(cell, schemaName) {
        const base = {
            schemaId: 0,
            schema: schemaName,
            id: cell.id,
            shape: cell.isNode() ? 'rect' : 'edge',
        };

        if (cell.isNode()) {
            const pos = cell.getPosition();
            return {
                ...base,
                data: { label: cell.getData()?.label ?? '' },
                position: { x: Math.round(pos.x), y: Math.round(pos.y) },
            };
        }
        
        const source = cell.getSource() || {};
        const target = cell.getTarget() || {};

        return {
            ...base,
            data: { load: cell.getData()?.load ?? 0 },
            source: {
                cell: source.cell,
                port: source.port ?? null,
            },
            target: {
                cell: target.cell,
                port: target.port ?? null,
            },
        };
    }

    async function saveSchema() {
        const name = els.schemaName.value.trim();
        if (!name) {
            notify('Введите название схемы', true);
            els.schemaName.focus();
            return;
        }

        const cells = graph.getCells();
        if (!cells.length) {
            notify('Схема пуста — добавьте станции и связи', true);
            return;
        }

        const payload = {
            id: 0,
            name,
            description: els.schemaDescription.value.trim(),
            cells: cells.map((cell) => serializeCell(cell, name)),
        };

        setLoading(true);
        try {
            const res = await fetch(SAVE_URL, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload),
            });
            if (!res.ok) throw new Error('HTTP ' + res.status);
            notify('Схема сохранена');
        } catch (err) {
            notify('Не удалось сохранить схему: ' + err.message, true);
        } finally {
            setLoading(false);
        }
    }

    function setLoading(isLoading) {
        els.saveBtn.classList.toggle('is-loading', isLoading);
        els.saveBtn.disabled = isLoading;
    }


    function bindEvents() {
        els.addStationBtn.addEventListener('click', addStation);
        els.saveBtn.addEventListener('click', saveSchema);

        els.clearBtn.addEventListener('click', () => {
            if (!graph.getCells().length) return;
            if (!confirm('Очистить схему? Все узлы и связи будут удалены.')) return;
            placedStations.clear();
            graph.clearCells();
            refreshStationOptions();
            closeEdgePanel();
        });

        els.zoomIn.addEventListener('click', () => graph.zoom(0.2));
        els.zoomOut.addEventListener('click', () => graph.zoom(-0.2));
        els.zoomFit.addEventListener('click', () => graph.zoomToFit({ padding: 40, maxScale: 1.5 }));
        els.zoomReset.addEventListener('click', () => {
            graph.zoomTo(1);
            graph.centerContent();
        });
        
        els.edgeDelete.addEventListener('click', () => {
            if (activeEdge) graph.removeEdge(activeEdge.id);
            closeEdgePanel();
        });

        document.addEventListener('keydown', (e) => {
            if (e.key !== 'Delete' && e.key !== 'Backspace') return;
            const t = e.target;
            if (t && ['INPUT', 'TEXTAREA', 'SELECT'].includes(t.tagName)) return;
            if (activeEdge) {
                graph.removeEdge(activeEdge.id);
                closeEdgePanel();
            } else if (selectedNode) {
                graph.removeNode(selectedNode.id);
            }
        });
    }

    function notify(message, isError) {
        clearTimeout(notifyTimer);
        els.notification.textContent = message;
        els.notification.classList.toggle('is-error', Boolean(isError));
        els.notification.classList.add('is-visible');
        notifyTimer = setTimeout(() => els.notification.classList.remove('is-visible'), 4000);
    }
})();