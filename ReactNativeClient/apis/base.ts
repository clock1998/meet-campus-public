export let baseApiUrl = process.env.EXPO_PUBLIC_API_URL!;
export let baseUrl = process.env.EXPO_PUBLIC_BASE_URL!;

export interface ApiError extends Error{
    detail:string,
    instance:string,
    requestId:string,
    status:number,
    title:string,
    traceId:string,
    type:string,
}

export let postOptions = {
    method: 'POST',
    headers: {
        'Content-Type':'application/json;charset=UTF-8'
    },
    body: JSON.stringify({})
}

export let getOptions = {
    method: 'GET',
    headers: {
        'Content-Type':'application/json;charset=UTF-8'
    }
}