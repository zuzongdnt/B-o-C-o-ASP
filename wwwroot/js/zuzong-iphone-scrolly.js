import * as THREE from 'https://esm.sh/three@0.164.1';
import { GLTFLoader } from 'https://esm.sh/three@0.164.1/examples/jsm/loaders/GLTFLoader.js?deps=three@0.164.1';

const root = document.getElementById('iphone17ProStory');
const canvas = document.getElementById('zzIphoneModel');
const colorModal = document.getElementById('iphone17ColorModal');
const colorCanvas = document.getElementById('zzIphoneColorModel');

if (root && canvas) {
    const modelWrap = root.querySelector('.zz-pro-model');
    const modelSrc = modelWrap?.dataset.modelSrc;
    const buttons = Array.from(root.querySelectorAll('[data-pro-step]'));
    const copyBlocks = Array.from(root.querySelectorAll('[data-pro-copy]'));
    const loading = root.querySelector('.zz-pro-model__loading');
    const modalModelWrap = colorModal?.querySelector('.zz-pro-color-modal__model');
    const modalLoading = colorModal?.querySelector('.zz-pro-color-modal__loading');
    const modalFallback = colorModal?.querySelector('.zz-pro-color-modal__fallback');
    const colorName = colorModal?.querySelector('[data-color-name]');
    const colorButtons = Array.from(colorModal?.querySelectorAll('[data-color-option]') || []);
    const closeColorModalButton = colorModal?.querySelector('.zz-pro-color-modal__close');

    const states = [
        { rotation: { x: 0.1, y: -0.55, z: -0.08 }, camera: 5.1, light: '#ff8a1f' },
        { rotation: { x: 0.05, y: -1.02, z: -0.04 }, camera: 4.85, light: '#ffd2a1' },
        { rotation: { x: 0.3, y: -0.72, z: 0.02 }, camera: 4.65, light: '#ff6900' },
        { rotation: { x: -0.02, y: 0.2, z: -0.06 }, camera: 4.9, light: '#fff5df' },
        { rotation: { x: -0.03, y: 2.78, z: 0.02 }, camera: 5.0, light: '#ff6900' },
        { rotation: { x: 0.04, y: 1.22, z: -0.02 }, camera: 4.75, light: '#ffb25c' },
        { rotation: { x: 0.16, y: -1.38, z: 0.08 }, camera: 4.8, light: '#ffffff' }
    ];

    const colorOptions = {
        orange: {
            name: 'Cam Vũ Trụ',
            src: modalModelWrap?.dataset.orangeSrc || modelSrc,
            fallback: '/images/ip17_promax_back.jpg',
            light: '#ff8a1f',
            rim: '#ffb25c',
            rotation: { x: 0.08, y: 2.46, z: -0.04 },
            scale: 3.1
        },
        silver: {
            name: 'Bạc',
            src: modalModelWrap?.dataset.silverSrc,
            fallback: '/images/ip17_pro_back.jpg',
            light: '#f7f5ee',
            rim: '#aeb4ba',
            rotation: { x: 0.08, y: 2.46, z: -0.04 },
            scale: 3.18
        }
    };

    let activeStep = 0;
    let model = null;
    let targetCameraZ = states[0].camera;
    let pointerX = 0;
    let pointerY = 0;
    let modalScene = null;
    let modalCamera = null;
    let modalRenderer = null;
    let modalGroup = null;
    let modalModel = null;
    let modalKeyLight = null;
    let modalRimLight = null;
    let modalTargetRotation = colorOptions.orange.rotation;
    let modalPointerX = 0;
    let modalPointerY = 0;
    let modalActiveColor = 'orange';
    let modalLoadToken = 0;
    const modalModelCache = new Map();

    const scene = new THREE.Scene();
    const camera = new THREE.PerspectiveCamera(34, 1, 0.1, 100);
    camera.position.set(0, 0.12, targetCameraZ);

    const renderer = new THREE.WebGLRenderer({
        canvas,
        alpha: true,
        antialias: true,
        powerPreference: 'high-performance'
    });
    renderer.outputColorSpace = THREE.SRGBColorSpace;
    renderer.toneMapping = THREE.ACESFilmicToneMapping;
    renderer.toneMappingExposure = 1.05;
    renderer.setPixelRatio(Math.min(window.devicePixelRatio || 1, 2));

    const keyLight = new THREE.DirectionalLight(states[0].light, 3.2);
    keyLight.position.set(4, 5, 5);
    scene.add(keyLight);

    const rimLight = new THREE.DirectionalLight('#ff8a1f', 1.75);
    rimLight.position.set(-5, 1.5, -3);
    scene.add(rimLight);

    const fillLight = new THREE.HemisphereLight('#ffffff', '#1f2937', 1.65);
    scene.add(fillLight);

    const modelGroup = new THREE.Group();
    scene.add(modelGroup);
    const loader = new GLTFLoader();

    function resizeRenderer() {
        const rect = canvas.getBoundingClientRect();
        const width = Math.max(1, rect.width);
        const height = Math.max(1, rect.height);
        renderer.setSize(width, height, false);
        camera.aspect = width / height;
        camera.updateProjectionMatrix();
    }

    function normalizeModel(object, scale = 2.95) {
        const bounds = new THREE.Box3().setFromObject(object);
        const size = bounds.getSize(new THREE.Vector3());
        const center = bounds.getCenter(new THREE.Vector3());
        const maxAxis = Math.max(size.x, size.y, size.z) || 1;

        object.position.sub(center);
        object.scale.setScalar(scale / maxAxis);
        object.traverse((child) => {
            if (child.isMesh) {
                child.castShadow = false;
                child.receiveShadow = false;
                if (child.material) {
                    child.material.envMapIntensity = 1.18;
                    child.material.needsUpdate = true;
                }
            }
        });
    }

    function activateStep(index, shouldScroll = false) {
        activeStep = Math.max(0, Math.min(states.length - 1, index));
        targetCameraZ = states[activeStep].camera;
        keyLight.color.set(states[activeStep].light);

        buttons.forEach((button, buttonIndex) => {
            button.classList.toggle('is-active', buttonIndex === activeStep);
        });

        copyBlocks.forEach((block, blockIndex) => {
            block.classList.toggle('is-active', blockIndex === activeStep);
        });

        if (shouldScroll) {
            copyBlocks[activeStep]?.scrollIntoView({ behavior: 'smooth', block: 'center' });
        }
    }

    buttons.forEach((button) => {
        button.addEventListener('click', () => {
            activateStep(Number(button.dataset.proStep || 0), true);
            if (button.dataset.proOpenColors === 'true') {
                openColorModal(modalActiveColor || 'orange');
            }
        });
    });

    const copyObserver = new IntersectionObserver((entries) => {
        const visibleEntry = entries
            .filter((entry) => entry.isIntersecting)
            .sort((a, b) => b.intersectionRatio - a.intersectionRatio)[0];

        if (visibleEntry) {
            activateStep(Number(visibleEntry.target.dataset.proCopy || 0));
        }
    }, {
        rootMargin: '-35% 0px -35% 0px',
        threshold: [0.2, 0.45, 0.7]
    });

    copyBlocks.forEach((block) => copyObserver.observe(block));

    root.addEventListener('pointermove', (event) => {
        const rect = root.getBoundingClientRect();
        pointerX = ((event.clientX - rect.left) / rect.width - 0.5) * 0.16;
        pointerY = ((event.clientY - rect.top) / rect.height - 0.5) * 0.12;
    });

    new ResizeObserver(resizeRenderer).observe(canvas);

    loader.load(modelSrc, (gltf) => {
        model = gltf.scene;
        normalizeModel(model, 2.95);
        model.rotation.set(states[0].rotation.x, states[0].rotation.y, states[0].rotation.z);

        modelGroup.add(model);
        root.classList.add('is-model-ready');
        loading?.remove();
        resizeRenderer();
    }, undefined, () => {
        root.classList.add('is-model-fallback');
        loading?.remove();
    });

    function setupColorModalScene() {
        if (!colorModal || !colorCanvas || modalRenderer) {
            return;
        }

        modalScene = new THREE.Scene();
        modalCamera = new THREE.PerspectiveCamera(27, 1, 0.1, 100);
        modalCamera.position.set(0.04, 0.08, 4.15);

        modalRenderer = new THREE.WebGLRenderer({
            canvas: colorCanvas,
            alpha: true,
            antialias: true,
            powerPreference: 'high-performance'
        });
        modalRenderer.outputColorSpace = THREE.SRGBColorSpace;
        modalRenderer.toneMapping = THREE.ACESFilmicToneMapping;
        modalRenderer.toneMappingExposure = 1.08;
        modalRenderer.setPixelRatio(Math.min(window.devicePixelRatio || 1, 2));

        modalKeyLight = new THREE.DirectionalLight(colorOptions.orange.light, 3.85);
        modalKeyLight.position.set(4.5, 4, 5);
        modalScene.add(modalKeyLight);

        modalRimLight = new THREE.DirectionalLight(colorOptions.orange.rim, 2.1);
        modalRimLight.position.set(-5.2, 1.6, -2.5);
        modalScene.add(modalRimLight);
        modalScene.add(new THREE.HemisphereLight('#ffffff', '#111111', 1.55));

        modalGroup = new THREE.Group();
        modalGroup.position.set(0.5, -0.18, 0);
        modalScene.add(modalGroup);

        new ResizeObserver(resizeColorModalRenderer).observe(colorCanvas);
    }

    function resizeColorModalRenderer() {
        if (!modalRenderer || !modalCamera || !colorCanvas) {
            return;
        }

        const rect = colorCanvas.getBoundingClientRect();
        const width = Math.max(1, rect.width);
        const height = Math.max(1, rect.height);
        modalRenderer.setSize(width, height, false);
        modalCamera.aspect = width / height;
        modalCamera.updateProjectionMatrix();
    }

    function setModalUi(colorKey) {
        const option = colorOptions[colorKey] || colorOptions.orange;
        modalActiveColor = colorKey;
        modalTargetRotation = option.rotation;

        colorModal?.setAttribute('data-active-color', colorKey);
        if (colorName) {
            colorName.textContent = option.name;
        }
        if (modalFallback && option.fallback) {
            modalFallback.src = option.fallback;
        }
        colorButtons.forEach((button) => {
            const isActive = button.dataset.colorOption === colorKey;
            button.classList.toggle('is-active', isActive);
            button.setAttribute('aria-pressed', String(isActive));
        });
        modalKeyLight?.color.set(option.light);
        modalRimLight?.color.set(option.rim);
    }

    function showModalModel(object) {
        if (!modalGroup) {
            return;
        }

        modalGroup.clear();
        modalModel = object;
        modalModel.rotation.set(modalTargetRotation.x, modalTargetRotation.y, modalTargetRotation.z);
        modalGroup.add(modalModel);
        colorModal?.classList.add('is-model-ready');
        colorModal?.classList.remove('is-model-fallback', 'is-model-loading');
        modalLoading?.classList.remove('is-visible');
        resizeColorModalRenderer();
    }

    function loadModalColor(colorKey) {
        const option = colorOptions[colorKey] || colorOptions.orange;
        if (!option.src) {
            colorModal?.classList.add('is-model-fallback');
            return;
        }

        setModalUi(colorKey);
        setupColorModalScene();
        colorModal?.classList.add('is-model-loading');
        colorModal?.classList.remove('is-model-ready', 'is-model-fallback');
        modalLoading?.classList.add('is-visible');

        const loadToken = ++modalLoadToken;

        if (modalModelCache.has(colorKey)) {
            showModalModel(modalModelCache.get(colorKey));
            return;
        }

        loader.load(option.src, (gltf) => {
            if (loadToken !== modalLoadToken) {
                return;
            }

            const loadedModel = gltf.scene;
            normalizeModel(loadedModel, option.scale || 3.5);
            loadedModel.traverse((child) => {
                if (child.isMesh && child.material) {
                    child.material.envMapIntensity = colorKey === 'silver' ? 1.35 : 1.22;
                    child.material.needsUpdate = true;
                }
            });
            modalModelCache.set(colorKey, loadedModel);
            showModalModel(loadedModel);
        }, undefined, () => {
            if (loadToken !== modalLoadToken) {
                return;
            }
            colorModal?.classList.add('is-model-fallback');
            colorModal?.classList.remove('is-model-ready', 'is-model-loading');
            modalLoading?.classList.remove('is-visible');
        });
    }

    function openColorModal(colorKey = 'orange') {
        if (!colorModal || !colorCanvas) {
            return;
        }

        setupColorModalScene();
        colorModal.classList.add('is-open');
        colorModal.setAttribute('aria-hidden', 'false');
        document.documentElement.classList.add('zz-pro-color-modal-open');
        requestAnimationFrame(() => {
            resizeColorModalRenderer();
            loadModalColor(colorKey);
            closeColorModalButton?.focus({ preventScroll: true });
        });
    }

    function closeColorModal() {
        if (!colorModal) {
            return;
        }

        colorModal.classList.remove('is-open');
        colorModal.setAttribute('aria-hidden', 'true');
        document.documentElement.classList.remove('zz-pro-color-modal-open');
    }

    colorButtons.forEach((button) => {
        button.addEventListener('click', () => {
            loadModalColor(button.dataset.colorOption || 'orange');
        });
    });

    closeColorModalButton?.addEventListener('click', closeColorModal);

    colorModal?.addEventListener('click', (event) => {
        if (event.target === colorModal) {
            closeColorModal();
        }
    });

    colorModal?.addEventListener('pointermove', (event) => {
        const rect = colorModal.getBoundingClientRect();
        modalPointerX = ((event.clientX - rect.left) / rect.width - 0.5) * 0.22;
        modalPointerY = ((event.clientY - rect.top) / rect.height - 0.5) * 0.16;
    });

    window.addEventListener('keydown', (event) => {
        if (event.key === 'Escape' && colorModal?.classList.contains('is-open')) {
            closeColorModal();
        }
    });

    window.addEventListener('resize', () => {
        resizeRenderer();
        resizeColorModalRenderer();
    });

    function animate() {
        requestAnimationFrame(animate);

        if (model) {
            const target = states[activeStep].rotation;
            model.rotation.x += (target.x + pointerY - model.rotation.x) * 0.065;
            model.rotation.y += (target.y + pointerX - model.rotation.y) * 0.065;
            model.rotation.z += (target.z - model.rotation.z) * 0.065;
        }

        modelGroup.position.y = Math.sin(performance.now() * 0.001) * 0.035;
        camera.position.z += (targetCameraZ - camera.position.z) * 0.06;
        camera.lookAt(0, 0, 0);
        renderer.render(scene, camera);

        if (modalRenderer && modalScene && modalCamera && colorModal?.classList.contains('is-open')) {
            if (modalModel) {
                modalModel.rotation.x += (modalTargetRotation.x + modalPointerY - modalModel.rotation.x) * 0.08;
                modalModel.rotation.y += (modalTargetRotation.y + modalPointerX - modalModel.rotation.y) * 0.08;
                modalModel.rotation.z += (modalTargetRotation.z - modalModel.rotation.z) * 0.08;
            }
            if (modalGroup) {
                modalGroup.position.y = -0.18 + Math.sin(performance.now() * 0.0012) * 0.025;
            }
            modalCamera.lookAt(0.36, 0, 0);
            modalRenderer.render(modalScene, modalCamera);
        }
    }

    resizeRenderer();
    activateStep(0);
    animate();
}
