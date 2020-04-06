/*
    SIDEBAR
        Filter
        Hver ár

    MAIN
        Samantekt á tímabili
            Upphaf og endir
            Fjöldi kvölda
            Fjöldi gesta
            Fjöldi unique gesta
            Fjöldi unique gestgjafa
            Fjöldi unique leikara
        Kvöld með flesta gesti
        Oftast mætt
        Oftast haldið
        Flest harðhausaverðlaun
        Vinsælasti leikarinn
        Flest kvöld í röð
        Gestgjafi með flest kvöld á einu ári
*/

import React from 'react';
import GuestStats from './guestStats';

const Statistics  = (propsData) => {
    // const[data, setData] = useState({rules: null, isLoading: false, visible: false})

    return (
        <div id="main">
            <GuestStats />
            {/* HostStats - https://ezhressapi.azurewebsites.net/api/hardhead/statistics/users?guestType=53&periodType=All&code=mIDqQM07DjZa7IkNtkapKigg9Edielksif1ODu49W13p3Xhsf70foQ== */}
        </div>        
    )
}

export default Statistics;