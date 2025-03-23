import { baseApiUrl, postOptions } from "./base";
export interface ApiError extends Error{
    detail:string,
    instance:string,
    requestId:string,
    status:number,
    title:string,
    traceId:string,
    type:string,
}
export async function login(data:any) {
    postOptions.body = JSON.stringify(data)
    const response = await fetch(`${baseApiUrl}/Auth/Login`, postOptions);
    if (!response.ok) {
        const errorData: ApiError = await response.json(); // if the API returns JSON error info
        throw errorData;
    }
    return await response.json();
}

export async function refresh(data:any) {
    postOptions.body = JSON.stringify(data)
    const response = await fetch(`${baseApiUrl}/Auth/RefreshToken`, postOptions);
    if (!response.ok) {
        const errorData: ApiError = await response.json(); // if the API returns JSON error info
        throw errorData;
    }
    return await response.json();
}