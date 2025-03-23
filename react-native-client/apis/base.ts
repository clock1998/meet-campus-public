export let baseApiUrl = process.env.EXPO_PUBLIC_API_URL!;
export let baseUrl = process.env.EXPO_PUBLIC_BASE_URL!;

export let postOptions = {
    method: 'POST',
    headers: {
        'Content-Type':'application/json;charset=UTF-8'
    },
    body: JSON.stringify({})
}