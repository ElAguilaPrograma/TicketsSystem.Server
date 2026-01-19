import { Injectable } from "@angular/core";

@Injectable({
    providedIn: 'root'
})
export class AuthService {
    isLoggedIn(): boolean {
        return localStorage.getItem('token') !== null;
    }

    login(): void {
        localStorage.setItem('token', "tokensimuladoXD");
    }

    logout(): void {
        localStorage.removeItem('token');
    }
}