import { definePreset } from '@primeuix/themes';
import Aura from '@primeuix/themes/aura';
import Material from '@primeuix/themes/material';
import Lara from '@primeuix/themes/lara';
import Nora from '@primeuix/themes/nora';

const createColorPreset = (colorName: string) => {
    return definePreset(Aura, {
        semantic: {
            primary: {
                50: `{${colorName}.50}`,
                100: `{${colorName}.100}`,
                200: `{${colorName}.200}`,
                300: `{${colorName}.300}`,
                400: `{${colorName}.400}`,
                500: `{${colorName}.500}`,
                600: `{${colorName}.600}`,
                700: `{${colorName}.700}`,
                800: `{${colorName}.800}`,
                900: `{${colorName}.900}`,
                950: `{${colorName}.950}`
            },
            colorScheme: {
                light: {
                    surface: {
                        0: '{slate.50}',
                        50: '{slate.100}',
                        100: '{slate.200}',
                        200: '{slate.300}',
                        300: '{slate.400}',
                        400: '{slate.500}',
                        500: '{slate.600}',
                        600: '{slate.700}',
                        700: '{slate.800}',
                        800: '{slate.900}',
                        900: '{slate.950}',
                        950: '{slate.950}'
                    },
                    primary: {
                        color: `{${colorName}.500}`,
                        inverseColor: '#ffffff',
                        hoverColor: `{${colorName}.600}`,
                        activeColor: `{${colorName}.700}`
                    },
                    highlight: {
                        background: `{${colorName}.50}`,
                        focusBackground: `{${colorName}.100}`,
                        color: `{${colorName}.700}`,
                        focusColor: `{${colorName}.800}`
                    }
                },
                dark: {
                    primary: {
                        color: `{${colorName}.400}`,
                        inverseColor: `{${colorName}.950}`,
                        hoverColor: `{${colorName}.300}`,
                        activeColor: `{${colorName}.200}`
                    },
                    highlight: {
                        background: `rgba(var(--p-${colorName}-500-rgb), .16)`,
                        focusBackground: `rgba(var(--p-${colorName}-500-rgb), .24)`,
                        color: `rgba(255,255,255,.87)`,
                        focusColor: `rgba(255,255,255,.87)`
                    }
                }
            }
        }
    });
};

export const Emerald = createColorPreset('emerald'); // Color por defecto de Aura
export const Blue = createColorPreset('blue');
export const Green = createColorPreset('green');
export const Indigo = createColorPreset('indigo');
export const Purple = createColorPreset('purple');
export const Teal = createColorPreset('teal');
export const Cyan = createColorPreset('cyan');
export const Orange = createColorPreset('orange');
export const Red = createColorPreset('red');
export const Pink = createColorPreset('pink');

export const Noir = definePreset(Aura, {
    semantic: {
        primary: {
            50: '{zinc.50}',
            100: '{zinc.100}',
            200: '{zinc.200}',
            300: '{zinc.300}',
            400: '{zinc.400}',
            500: '{zinc.500}',
            600: '{zinc.600}',
            700: '{zinc.700}',
            800: '{zinc.800}',
            900: '{zinc.900}',
            950: '{zinc.950}'
        },
        colorScheme: {
            light: {
                surface: {
                    0: '{slate.50}',
                    50: '{slate.100}',
                    100: '{slate.200}',
                    200: '{slate.300}',
                    300: '{slate.400}',
                    400: '{slate.500}',
                    500: '{slate.600}',
                    600: '{slate.700}',
                    700: '{slate.800}',
                    800: '{slate.900}',
                    900: '{slate.950}',
                    950: '{slate.950}'
                },
                primary: {
                    color: '{zinc.950}',
                    inverseColor: '#ffffff',
                    hoverColor: '{zinc.900}',
                    activeColor: '{zinc.800}'
                },
                highlight: {
                    background: '{zinc.200}',
                    focusBackground: '{zinc.300}',
                    color: '{zinc.900}',
                    focusColor: '{zinc.950}'
                },
                list: {
                    option: {
                        focusBackground: '{zinc.500}',
                        selectedBackground: '{zinc.600}',
                        selectedFocusBackground: '{zinc.700}'
                    }
                }
            },
            dark: {
                primary: {
                    color: '{zinc.100}',
                    inverseColor: '{zinc.950}',
                    hoverColor: '{zinc.100}',
                    activeColor: '{zinc.200}'
                },
                highlight: {
                    background: 'rgba(250, 250, 250, .16)',
                    focusBackground: 'rgba(250, 250, 250, .24)',
                    color: 'rgba(255,255,255,.87)',
                    focusColor: 'rgba(255,255,255,.87)'
                }
            }
        }
    }
});

export const MyPresets: { [key: string]: any } = {
    Noir,
    Emerald,
    Green,
    Blue,
    Indigo,
    Purple,
    Teal,
    Cyan,
    Orange,
    Red,
    Pink
};