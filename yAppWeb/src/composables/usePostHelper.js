import { get } from 'aws-amplify/api';
import { onMounted, ref } from 'vue';

export function usePostHelper( apiPath ) 
{
    const jsonData = ref([]); // Reactive array to hold the list of posts
    const maxLength = 100;
    const loading = ref(true); // Reactive boolean for loading spinner
    
    // Get list of most recent posts as JSON
    onMounted(async () => 
    {
        getPosts();
    });
    
    async function getPosts() 
    {
        try 
        {
            const restOperation = await get({
                apiName: 'yapp',
                path: `${apiPath}`
            });
            const { body } = await restOperation.response;
            const response = await ((await body.blob()).arrayBuffer());
            const decoder = new TextDecoder('utf-8'); // Use TextDecoder to decode the ArrayBuffer to a string
            const decodedText = decoder.decode(response);
            jsonData.value = JSON.parse(decodedText); // Update with parsed JSON
            loading.value = false;
        } 
        catch(error)
        {
            console.log('GET call failed', error);
        }
        if ( jsonData.value.length > 0 )
        {
            const tabContent = document.querySelector(".TabsContent .Text");
            if ( tabContent )
            {
               tabContent.innerHTML = "";
            }
            
        }
    }
    
    function truncateText(text) 
    {
        var modifiedText = text
        if ( text.length > maxLength ) 
        {
            modifiedText = text.substring(0, maxLength) + "...";
        }
        return modifiedText;
    }
    
    function updatePath(newPath)
    {
        apiPath = newPath;
    }

    return { jsonData, loading, truncateText, getPosts, updatePath };
}
