import { Routes } from '@angular/router';
import { HomeComponent } from './pages/home/home.component';
import { MissionComponent } from './pages/mission/mission.component';
import { AuthorizeGuard } from './shared/service/authorize-guard.service';
import { MeetComponent } from './pages/meet/meet.component';
import { LoginComponent } from './pages/login/login.component';
import { AboutComponent } from './pages/about/about.component';

export const routes: Routes = [
    { path: '', component: HomeComponent, title: 'Home' },
    { path: 'Mission', component: MissionComponent, title: 'Mission' },
    { path: 'Login', component: LoginComponent, title: 'Login' },
    { path: 'About', component: AboutComponent, title: 'About' },
    { path: 'Meet', component: MeetComponent, title: 'Meet', canActivate: [AuthorizeGuard] },
];
