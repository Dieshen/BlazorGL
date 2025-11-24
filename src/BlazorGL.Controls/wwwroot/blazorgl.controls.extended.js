// BlazorGL Extended Controls - JavaScript Interop
// Handles TrackballControls, TransformControls, and DragControls

const controlsState = new Map();

// Mouse button constants
const MOUSE_BUTTON = {
    LEFT: 0,
    MIDDLE: 1,
    RIGHT: 2
};

// Control states
const STATE = {
    NONE: -1,
    ROTATE: 0,
    ZOOM: 1,
    PAN: 2,
    TOUCH_ROTATE: 3,
    TOUCH_ZOOM_PAN: 4
};

// ============================================================================
// TrackballControls
// ============================================================================

export function initTrackballControls(domElementId, dotNetRef) {
    const element = document.getElementById(domElementId);
    if (!element) {
        console.error(`Element with id '${domElementId}' not found`);
        return;
    }

    const state = {
        dotNetRef: dotNetRef,
        element: element,
        currentState: STATE.NONE,
        pointers: [],
        pointerPositions: {}
    };

    // Event handlers
    const onPointerDown = (event) => handleTrackballPointerDown(event, state);
    const onPointerMove = (event) => handleTrackballPointerMove(event, state);
    const onPointerUp = (event) => handleTrackballPointerUp(event, state);
    const onPointerCancel = (event) => handleTrackballPointerUp(event, state);
    const onContextMenu = (event) => event.preventDefault();
    const onWheel = (event) => handleTrackballWheel(event, state);

    // Attach event listeners
    element.addEventListener('pointerdown', onPointerDown);
    element.addEventListener('pointermove', onPointerMove);
    element.addEventListener('pointerup', onPointerUp);
    element.addEventListener('pointercancel', onPointerCancel);
    element.addEventListener('contextmenu', onContextMenu);
    element.addEventListener('wheel', onWheel, { passive: false });

    // Store state and handlers for cleanup
    state.handlers = {
        onPointerDown,
        onPointerMove,
        onPointerUp,
        onPointerCancel,
        onContextMenu,
        onWheel
    };

    controlsState.set(`trackball_${domElementId}`, state);
}

export function disposeTrackballControls(domElementId) {
    const state = controlsState.get(`trackball_${domElementId}`);
    if (!state) return;

    const element = state.element;
    const handlers = state.handlers;

    // Remove event listeners
    element.removeEventListener('pointerdown', handlers.onPointerDown);
    element.removeEventListener('pointermove', handlers.onPointerMove);
    element.removeEventListener('pointerup', handlers.onPointerUp);
    element.removeEventListener('pointercancel', handlers.onPointerCancel);
    element.removeEventListener('contextmenu', handlers.onContextMenu);
    element.removeEventListener('wheel', handlers.onWheel);

    controlsState.delete(`trackball_${domElementId}`);
}

function handleTrackballPointerDown(event, state) {
    if (state.pointers.length === 0) {
        state.element.setPointerCapture(event.pointerId);
    }

    addPointer(event, state);

    if (event.pointerType === 'touch') {
        handleTrackballTouchStart(event, state);
    } else {
        handleTrackballMouseDown(event, state);
    }
}

function handleTrackballPointerMove(event, state) {
    if (event.pointerType === 'touch') {
        handleTrackballTouchMove(event, state);
    } else {
        handleTrackballMouseMove(event, state);
    }
}

function handleTrackballPointerUp(event, state) {
    removePointer(event, state);

    if (state.pointers.length === 0) {
        state.element.releasePointerCapture(event.pointerId);
        state.currentState = STATE.NONE;
    }
}

function handleTrackballMouseDown(event, state) {
    let mouseAction = STATE.NONE;

    switch (event.button) {
        case MOUSE_BUTTON.LEFT:
            mouseAction = STATE.ROTATE;
            state.dotNetRef.invokeMethodAsync('OnRotateStart', event.clientX, event.clientY);
            break;
        case MOUSE_BUTTON.MIDDLE:
            mouseAction = STATE.ZOOM;
            state.dotNetRef.invokeMethodAsync('OnZoomStart', event.clientX, event.clientY);
            break;
        case MOUSE_BUTTON.RIGHT:
            mouseAction = STATE.PAN;
            state.dotNetRef.invokeMethodAsync('OnPanStart', event.clientX, event.clientY);
            break;
    }

    state.currentState = mouseAction;
}

function handleTrackballMouseMove(event, state) {
    if (state.currentState === STATE.ROTATE) {
        state.dotNetRef.invokeMethodAsync('OnRotateMove', event.clientX, event.clientY);
    } else if (state.currentState === STATE.ZOOM) {
        state.dotNetRef.invokeMethodAsync('OnZoomMove', event.clientX, event.clientY);
    } else if (state.currentState === STATE.PAN) {
        state.dotNetRef.invokeMethodAsync('OnPanMove', event.clientX, event.clientY);
    }
}

function handleTrackballTouchStart(event, state) {
    trackPointer(event, state);

    switch (state.pointers.length) {
        case 1:
            // Single touch - rotate
            state.currentState = STATE.TOUCH_ROTATE;
            state.dotNetRef.invokeMethodAsync('OnRotateStart', event.clientX, event.clientY);
            break;

        case 2:
            // Two touches - zoom and pan
            state.currentState = STATE.TOUCH_ZOOM_PAN;
            const x = (state.pointers[0].pageX + state.pointers[1].pageX) * 0.5;
            const y = (state.pointers[0].pageY + state.pointers[1].pageY) * 0.5;
            state.dotNetRef.invokeMethodAsync('OnZoomStart', x, y);
            state.dotNetRef.invokeMethodAsync('OnPanStart', x, y);
            break;
    }
}

function handleTrackballTouchMove(event, state) {
    trackPointer(event, state);

    switch (state.pointers.length) {
        case 1:
            if (state.currentState === STATE.TOUCH_ROTATE) {
                state.dotNetRef.invokeMethodAsync('OnRotateMove', event.clientX, event.clientY);
            }
            break;

        case 2:
            if (state.currentState === STATE.TOUCH_ZOOM_PAN) {
                const x = (state.pointers[0].pageX + state.pointers[1].pageX) * 0.5;
                const y = (state.pointers[0].pageY + state.pointers[1].pageY) * 0.5;
                state.dotNetRef.invokeMethodAsync('OnZoomMove', x, y);
                state.dotNetRef.invokeMethodAsync('OnPanMove', x, y);
            }
            break;
    }
}

function handleTrackballWheel(event, state) {
    event.preventDefault();

    const delta = event.deltaY !== 0 ? event.deltaY : event.deltaX;
    state.dotNetRef.invokeMethodAsync('OnZoomStart', event.clientX, event.clientY);
    state.dotNetRef.invokeMethodAsync('OnZoomMove', event.clientX, event.clientY + delta);
}

// ============================================================================
// TransformControls
// ============================================================================

export function initTransformControls(domElementId, dotNetRef) {
    const element = document.getElementById(domElementId);
    if (!element) {
        console.error(`Element with id '${domElementId}' not found`);
        return;
    }

    const state = {
        dotNetRef: dotNetRef,
        element: element,
        dragging: false,
        axis: null
    };

    // Event handlers
    const onPointerDown = (event) => handleTransformPointerDown(event, state);
    const onPointerMove = (event) => handleTransformPointerMove(event, state);
    const onPointerUp = (event) => handleTransformPointerUp(event, state);
    const onPointerCancel = (event) => handleTransformPointerUp(event, state);
    const onContextMenu = (event) => event.preventDefault();

    // Attach event listeners
    element.addEventListener('pointerdown', onPointerDown);
    element.addEventListener('pointermove', onPointerMove);
    element.addEventListener('pointerup', onPointerUp);
    element.addEventListener('pointercancel', onPointerCancel);
    element.addEventListener('contextmenu', onContextMenu);

    // Store state and handlers for cleanup
    state.handlers = {
        onPointerDown,
        onPointerMove,
        onPointerUp,
        onPointerCancel,
        onContextMenu
    };

    controlsState.set(`transform_${domElementId}`, state);
}

export function disposeTransformControls(domElementId) {
    const state = controlsState.get(`transform_${domElementId}`);
    if (!state) return;

    const element = state.element;
    const handlers = state.handlers;

    // Remove event listeners
    element.removeEventListener('pointerdown', handlers.onPointerDown);
    element.removeEventListener('pointermove', handlers.onPointerMove);
    element.removeEventListener('pointerup', handlers.onPointerUp);
    element.removeEventListener('pointercancel', handlers.onPointerCancel);
    element.removeEventListener('contextmenu', handlers.onContextMenu);

    controlsState.delete(`transform_${domElementId}`);
}

function handleTransformPointerDown(event, state) {
    // Check if pointer is over gizmo axis (would need raycasting in real implementation)
    // For now, assume any left click starts transform on Y axis
    if (event.button === MOUSE_BUTTON.LEFT) {
        state.dragging = true;
        state.axis = 'Y'; // Default axis, should be determined by raycasting

        const rect = state.element.getBoundingClientRect();
        const x = ((event.clientX - rect.left) / rect.width) * 2 - 1;
        const y = -((event.clientY - rect.top) / rect.height) * 2 + 1;

        state.dotNetRef.invokeMethodAsync('OnPointerDown', x, y, state.axis);
    }
}

function handleTransformPointerMove(event, state) {
    if (state.dragging) {
        const rect = state.element.getBoundingClientRect();
        const x = ((event.clientX - rect.left) / rect.width) * 2 - 1;
        const y = -((event.clientY - rect.top) / rect.height) * 2 + 1;

        state.dotNetRef.invokeMethodAsync('OnPointerMove', x, y);
    }
}

function handleTransformPointerUp(event, state) {
    if (state.dragging) {
        state.dragging = false;
        state.axis = null;
        state.dotNetRef.invokeMethodAsync('OnPointerUp');
    }
}

// ============================================================================
// DragControls
// ============================================================================

export function initDragControls(domElementId, dotNetRef) {
    const element = document.getElementById(domElementId);
    if (!element) {
        console.error(`Element with id '${domElementId}' not found`);
        return;
    }

    const state = {
        dotNetRef: dotNetRef,
        element: element,
        dragging: false
    };

    // Event handlers
    const onPointerDown = (event) => handleDragPointerDown(event, state);
    const onPointerMove = (event) => handleDragPointerMove(event, state);
    const onPointerUp = (event) => handleDragPointerUp(event, state);
    const onPointerCancel = (event) => handleDragPointerUp(event, state);

    // Attach event listeners
    element.addEventListener('pointerdown', onPointerDown);
    element.addEventListener('pointermove', onPointerMove);
    element.addEventListener('pointerup', onPointerUp);
    element.addEventListener('pointercancel', onPointerCancel);

    // Store state and handlers for cleanup
    state.handlers = {
        onPointerDown,
        onPointerMove,
        onPointerUp,
        onPointerCancel
    };

    controlsState.set(`drag_${domElementId}`, state);
}

export function disposeDragControls(domElementId) {
    const state = controlsState.get(`drag_${domElementId}`);
    if (!state) return;

    const element = state.element;
    const handlers = state.handlers;

    // Remove event listeners
    element.removeEventListener('pointerdown', handlers.onPointerDown);
    element.removeEventListener('pointermove', handlers.onPointerMove);
    element.removeEventListener('pointerup', handlers.onPointerUp);
    element.removeEventListener('pointercancel', handlers.onPointerCancel);

    controlsState.delete(`drag_${domElementId}`);
}

function handleDragPointerDown(event, state) {
    if (event.button === MOUSE_BUTTON.LEFT) {
        state.element.setPointerCapture(event.pointerId);
        state.dragging = true;

        const rect = state.element.getBoundingClientRect();
        const x = ((event.clientX - rect.left) / rect.width) * 2 - 1;
        const y = -((event.clientY - rect.top) / rect.height) * 2 + 1;

        state.dotNetRef.invokeMethodAsync('OnPointerDown', x, y);
    }
}

function handleDragPointerMove(event, state) {
    const rect = state.element.getBoundingClientRect();
    const x = ((event.clientX - rect.left) / rect.width) * 2 - 1;
    const y = -((event.clientY - rect.top) / rect.height) * 2 + 1;

    state.dotNetRef.invokeMethodAsync('OnPointerMove', x, y);
}

function handleDragPointerUp(event, state) {
    if (state.dragging) {
        state.element.releasePointerCapture(event.pointerId);
        state.dragging = false;
        state.dotNetRef.invokeMethodAsync('OnPointerUp');
    }
}

// ============================================================================
// Shared Helper Functions
// ============================================================================

function addPointer(event, state) {
    state.pointers.push(event);
}

function removePointer(event, state) {
    delete state.pointerPositions[event.pointerId];

    for (let i = 0; i < state.pointers.length; i++) {
        if (state.pointers[i].pointerId === event.pointerId) {
            state.pointers.splice(i, 1);
            return;
        }
    }
}

function trackPointer(event, state) {
    let position = state.pointerPositions[event.pointerId];

    if (position === undefined) {
        position = { x: 0, y: 0 };
        state.pointerPositions[event.pointerId] = position;
    }

    position.x = event.pageX;
    position.y = event.pageY;
}
