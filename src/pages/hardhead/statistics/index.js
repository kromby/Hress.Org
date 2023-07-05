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
        [X] - Kvöld með flesta gesti
        [X] - Oftast mætt
        [X] - Oftast haldið
        [ ] - Flest harðhausaverðlaun
        [X] - Vinsælasti leikarinn
        [ ] - Flest kvöld í röð
        [ ] - Gestgjafi með flest kvöld á einu ári
*/

import React, { useEffect } from 'react';
import GuestStats from './guestStats';
import HostStats from './hostStats';
import ActorStats from './actorStats';
import AttendanceStats from './attendanceStats';

const Statistics  = (propsData) => {
    useEffect(() => {
        document.title = "Tölfræði Harðhausa | Hress.Org";
    }, [propsData])

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