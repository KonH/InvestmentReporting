<template>
	<b>{{ asset.ticker }} {{ asset.name }} x{{ asset.count }}</b>
</template>
<script lang="ts">
import { Options, Vue } from 'vue-class-component';
import { AssetDto } from '@/api/state';
import { Prop } from 'vue-property-decorator';

@Options({
	name: 'Asset',
})
export default class Asset extends Vue {
	@Prop()
	asset!: AssetDto;
}
</script>
