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
        [ ] - Kvöld með flesta gesti
        [X] - Oftast mætt
        [X] - Oftast haldið
        [ ] - Flest harðhausaverðlaun
        [ ] - Vinsælasti leikarinn
        [ ] - Flest kvöld í röð
        [ ] - Gestgjafi með flest kvöld á einu ári
*/

import React from 'react';
import GuestStats from './guestStats';
import HostStats from './hostStats';
import ActorStats from './actorStats';
import AttendanceStats from './attendanceStats';

const Statistics  = (propsData) => {
    // const[data, setData] = useState({rules: null, isLoading: false, visible: false})

    return (
        <div id="main">
            <GuestStats />
            <HostStats />
            <ActorStats />
            <AttendanceStats />
        </div>        
    )
}

export default Statistics;