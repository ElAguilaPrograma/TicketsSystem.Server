import { Injectable, OnInit } from "@angular/core";

@Injectable({
    providedIn: 'root'
})
export class DarkModeService implements OnInit {
    darkMode: boolean = false;
    private readonly storageKey = 'dark-mode'

    ngOnInit(): void {
        const saved = localStorage.getItem(this.storageKey);
        const enabled = saved === null ? true : saved === 'true';
        this.darkMode = enabled;
        const root = document.documentElement;

        if (enabled) {
            root.classList.add('my-app-dark');
        } else {
            root.classList.remove('my-app-dark');
        }
    }

    toogleDarkMode(): void {
        const root = document.documentElement;
        const now = root.classList.toggle('my-app-dark');
        this.darkMode = now;
        try {
            localStorage.setItem(this.storageKey, now ? 'true' : 'false');
        } catch (error) {
            alert('Error saving dark mode preference');
        }
    }

    darkModeEnabled(): boolean {
        return this.darkMode;
    }

}