<script lang="ts">
  import { DATA_HOST } from "../libs/config";
  import { getMaimai, getMaimaiAllMusic } from "../libs/maimai";
  import type { ParsedRating, Rating } from "../libs/maimaiTypes";
  import { getMult } from "../libs/scoring";
  import StatusOverlays from "../components/StatusOverlays.svelte";

  export let userId: any
  userId = +userId
  let error: string | null;

  if (!userId) console.error("No user ID provided")

  Promise.all([
    getMaimai("GetUserRatingApi", {userId}),
    getMaimaiAllMusic()
  ]).then(([rating, music]) => {
    data = rating
    musicInfo = music

    if (!data || !musicInfo) {
      console.error("Failed to fetch data")
      return
    }

    parsedRatings = {
      old: parseRating(data.userRating.ratingList),
      new: parseRating(data.userRating.newRatingList)
    }
  }).catch((e) => error = e.message)

  function parseRating(arr: Rating[]) {
    return arr.map(x => {
      const music = musicInfo[x.musicId]

      if (!music) {
        console.error(`Music not found: ${x.musicId}`)
        return null
      }

      music.note = music.notes[x.level]
      const mult = getMult(x.achievement, 'mai2')
      return {
        ...x,
        music: music,
        calc: (mult[1] as number) * music.note.lv,
        rank: mult[2]
      }
    }).filter(x => x != null) as ParsedRating[]
  }

  let parsedRatings: {
    old: ParsedRating[],
    new: ParsedRating[]
  } | null = null

  let data: {
    userRating: {
      rating: number,
      ratingList: Rating[],
      newRatingList: Rating[]
    }
  } | null = null

  let musicInfo: any = null
</script>

<main>
  <!-- Display all parsed ratings -->
  {#if parsedRatings}
    {#each [{title: "Old", data: parsedRatings.old}, {title: "New", data: parsedRatings.new}] as section}
      <h2>{section.title}</h2>
      <div class="rating-cards">
        {#each section.data as rating}
          <div class="level-{rating.level}">
            <img class="cover"
                 src={`${DATA_HOST}/maimai/assetbundle/jacket_s/00${rating.musicId.toString().padStart(6, '0').substring(2)}.png`}
                 alt="">

            <div class="detail">
              <span class="name">{rating.music.name}</span>
              <span class="rating">
              <span>{(rating.achievement / 10000).toFixed(2)}%</span>
              <img class="rank" src={`${DATA_HOST}/maimai/sprites/rankimage/UI_GAM_Rank_${rating.rank}.png`} alt="">
            </span>
              <span>{rating.calc.toFixed(1)}</span>
            </div>
            <img class="ver"
                 src={`${DATA_HOST}/maimai/sprites/tab/title/UI_CMN_TabTitle_MaimaiTitle_Ver${rating.music.ver.toString().substring(0, 3)}.png`}
                 alt="">
            <div class="lv">{rating.music.note.lv}</div>
          </div>
        {/each}
      </div>
    {/each}
  {/if}

  <StatusOverlays error={error} loading={!parsedRatings} />
</main>

<style lang="sass">
  .rating-cards
    display: grid
    gap: 2rem
    width: 100%

    // Fill as many columns as possible
    grid-template-columns: repeat(auto-fill, minmax(200px, 1fr))

    // Center the cards
    justify-items: center
    align-items: center

    // Style each card
    > div
      $border-radius: 20px
      width: 200px
      height: 200px
      border-radius: $border-radius

      display: flex
      position: relative

      // Difficulty border
      border: 5px solid var(--lv-color)

      img
        object-fit: cover
        pointer-events: none

      img.cover
        width: 100%
        height: 100%
        border-radius: calc($border-radius - 3px)

      img.ver
        position: absolute
        top: -20px
        left: -30px
        height: 50px

      // Information
      .detail
        position: absolute
        bottom: 0
        left: 0
        right: 0
        padding: 10px
        background: rgba(0, 0, 0, 0.5)
        border-radius: 0 0 calc($border-radius - 3px) calc($border-radius - 3px)

        // Blur
        backdrop-filter: blur(3px)

        display: flex
        flex-direction: column
        text-align: left

        > span
          // Disable text wrapping, max 2 lines
          overflow: hidden
          text-overflow: ellipsis
          white-space: nowrap

        .name
          font-size: 1.2em
          font-weight: bold

        .rating
          display: flex

          img
            height: 1.5em

      .lv
        position: absolute
        bottom: 0
        right: 0
        padding: 5px 10px
        background: rgb(var(--lv-color))
        // Top left border radius
        border-radius: 10px 0

        font-size: 1.3em

        &:before
          content: "Lv"
          font-size: 0.8em

    // Mobile
    @media (max-width: 500px)
      margin-left: -1rem
      margin-right: -1rem
      width: calc(100% + 2rem)
      grid-template-columns: repeat(auto-fill, minmax(130px, 1fr))
      font-size: 0.8em
      > div
        width: 150px
        height: 150px

        img.ver
          height: 45px
          left: -20px
</style>
