import { inject, Injectable } from '@angular/core';
import { environment } from '../../../environment';
import { HttpClient } from '@angular/common/http';
import { lastValueFrom } from 'rxjs';


export interface TracksRequestTrack {
  location: string;
  mid?: string | null;
  trackName?: string;
  sessionId?: string;
}

export interface Session {
  sessionId: string;
  sessionDescription: RTCSessionDescription;
}
export interface GetTracksBySessionIdResponse {
  sessionId: string;
  tracks: GetTracksBySessionIdResponseTrack[];
}
export interface GetTracksBySessionIdResponseTrack {
  location: string;
  mid: string;
  sessionId?: string;
  trackName: string;
  status: string;
}

export interface TracksResponse {
  requiresImmediateRenegotiation: boolean;
  tracks: Track[];
  sessionDescription: RTCSessionDescriptionInit;
}

export interface Track {
  sessionId?: string;
  trackName: string;
  mid: string;
}

@Injectable({
  providedIn: 'root'
})
export class CloudflareService {
  private http = inject(HttpClient);

  constructor() {
  }
  newSession(sdp: string): Promise<Session> {
    return lastValueFrom(this.http.post<Session>(`${environment.apiUrl}/Cloudflare/NewSession`, { sdp }));
  }
  newTracks(sessionId: string, tracks: TracksRequestTrack[], sdp?: string): Promise<TracksResponse> {
    return lastValueFrom(this.http.post<any>(`${environment.apiUrl}/Cloudflare/NewTracks`, { sessionId, tracks, sdp }));
  }

  sendAnswerSDP(sdp: string, sessionId: string): Promise<string> {
    return lastValueFrom(this.http.put<any>(`${environment.apiUrl}/Cloudflare/SendAnswerSDP`, { sdp, sessionId }));
  }

  closeTracks(sessionId: string, tracks: string[], sdp?: string, force?: boolean): Promise<any> {
    return lastValueFrom(this.http.put(`${environment.apiUrl}/Cloudflare/CloseTracks`, {sessionId, tracks, sdp, force}));
  }

  getSessionState(sessionId: string): Promise<GetTracksBySessionIdResponse> {
    return lastValueFrom(this.http.get<GetTracksBySessionIdResponse>(`${environment.apiUrl}/Cloudflare/GetSessionState/${sessionId}`));
  }
}
