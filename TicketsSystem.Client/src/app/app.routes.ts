import { Routes } from '@angular/router';
import { MainLayout } from './layout/main-layout/main-layout';
import { Home } from './features/home/home';
import { Login } from './features/auth/pages/login/login';
import { Main } from './features/tickets/pages/main/main';

export const routes: Routes = [
    { path: "", redirectTo: "home", pathMatch: "full" },
    { path: "login", component: Login },

    {
        path: "",
        component: MainLayout,
        children: [
            { path: "home", component: Home },
            { path: "main", component: Main },
        ]
    }
];
