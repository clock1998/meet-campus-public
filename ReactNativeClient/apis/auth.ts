import { ApiError, baseApiUrl, postOptions } from "./base";
import { UserSession } from "@/context/AuthContext";

export interface LoginCredentials {
    username: string;
    password: string;
}

export async function login(data: LoginCredentials): Promise<UserSession> {
    postOptions.body = JSON.stringify(data)
    const response = await fetch(`${baseApiUrl}/Auth/Login`, postOptions);
    if (!response.ok) {
        const errorData: ApiError = await response.json(); // if the API returns JSON error info
        throw errorData;
    }
    return await response.json();
}

export async function refresh(data: { refreshToken: string }): Promise<UserSession> {
    postOptions.body = JSON.stringify(data)
    const response = await fetch(`${baseApiUrl}/Auth/RefreshToken`, postOptions);
    if (!response.ok) {
        const errorData: ApiError = await response.json(); // if the API returns JSON error info
        throw errorData;
    }
    return await response.json();
}