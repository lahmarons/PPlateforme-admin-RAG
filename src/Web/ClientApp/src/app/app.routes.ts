import { Routes } from '@angular/router';
import { HomeComponent } from './home/home.component';
import { CounterComponent } from './counter/counter.component';
import { FetchDataComponent } from './fetch-data/fetch-data.component';
import { InscriptionComponent } from './citoyen/inscription/inscription.component';
import { ConnexionComponent } from './citoyen/connexion/connexion.component';
import { DashboardComponent } from './citoyen/dashboard/dashboard.component';
import { ServicesComponent } from './citoyen/services/services.component';
import { MesDemandesComponent } from './citoyen/demandes/mes-demandes.component';
import { NouvelleDemandeComponent } from './citoyen/demandes/nouvelle-demande.component';
import { ChatbotComponent } from './citoyen/chatbot/chatbot.component';
import { AgentConnexionComponent } from './agent/connexion/agent-connexion.component';
import { AgentDashboardComponent } from './agent/dashboard/agent-dashboard.component';
import { AgentDemandesComponent } from './agent/demandes/agent-demandes.component';
import { AgentChatbotComponent } from './agent/chatbot/agent-chatbot.component';
import { AdminConnexionComponent } from './admin/connexion/admin-connexion.component';
import { AdminDashboardComponent } from './admin/dashboard/admin-dashboard.component';
import { AdminServicesComponent } from './admin/services/admin-services.component';
import { AdminAgentsComponent } from './admin/agents/admin-agents.component';
import { AdminPrestatairesComponent } from './admin/prestataires/admin-prestataires.component';
import { PrestataireListComponent } from './prestataire/client/prestataire-list.component';
import { AppLayout } from './layout/component/app.layout';
import { ConnexionHubComponent } from './connexion/connexion.component';
import { AuthErrorComponent } from './components/auth-error/auth-error.component';
import { citoyenGuard, agentGuard, adminGuard } from './shared/guards/auth.guard';

export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'connexion', component: ConnexionHubComponent },
  { path: 'auth/error', component: AuthErrorComponent },
  {
    path: '',
    component: AppLayout,
    children: [
      { path: 'counter', component: CounterComponent },
      { path: 'fetch-data', component: FetchDataComponent },
      { path: 'citoyen/dashboard', component: DashboardComponent, canActivate: [citoyenGuard] },
      { path: 'citoyen/services', component: ServicesComponent, canActivate: [citoyenGuard] },
      { path: 'citoyen/demandes', component: MesDemandesComponent, canActivate: [citoyenGuard] },
      { path: 'citoyen/demandes/nouvelle', component: NouvelleDemandeComponent, canActivate: [citoyenGuard] },
      { path: 'citoyen/chatbot', component: ChatbotComponent, canActivate: [citoyenGuard] },
      { path: 'agent/dashboard', component: AgentDashboardComponent, canActivate: [agentGuard] },
      { path: 'agent/demandes', component: AgentDemandesComponent, canActivate: [agentGuard] },
      { path: 'agent/chatbot', component: AgentChatbotComponent, canActivate: [agentGuard] },
      { path: 'admin/dashboard', component: AdminDashboardComponent, canActivate: [adminGuard] },
      { path: 'admin/services', component: AdminServicesComponent, canActivate: [adminGuard] },
      { path: 'admin/agents', component: AdminAgentsComponent, canActivate: [adminGuard] },
      { path: 'admin/prestataires', component: AdminPrestatairesComponent, canActivate: [adminGuard] },
      { path: 'prestataires', component: PrestataireListComponent }
    ]
  },
  { path: 'citoyen/inscription', component: InscriptionComponent },
  { path: 'citoyen/connexion', component: ConnexionComponent },
  { path: 'agent/connexion', component: AgentConnexionComponent },
  { path: 'admin/connexion', component: AdminConnexionComponent },
  { path: '**', redirectTo: 'connexion' }
];
